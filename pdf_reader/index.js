const express = require("express");
const multer = require("multer");
const pdfParse = require("pdf-parse");
const fs = require("fs");
const axios = require("axios");
const xlsx = require("xlsx");

const app = express();
const port = 3000;

const URL_ADD_PDF_PRODUCTION = "http://localhost:5678/webhook/add-document";
const URL_ADD_PDF_TEST = "http://localhost:5678/webhook/add-document";
const URL_ADD_EXCEL_TEST = "http://localhost:5678/webhook-test/add_excel";
const URL_ADD_EXCEL_PRODUCTION = "http://localhost:5678/webhook/add-document";

// Configurazione multer per upload (usa la stessa cartella 'uploads/')
const upload = multer({ dest: "uploads/" });

// Middleware per servire file statici (public/)
app.use(express.static("public"));

// --- Endpoint esistente per PDF (invariato salvo qualche piccola pulizia) ---
app.post("/upload", upload.array("pdfs", 10), async (req, res) => {
  try {
    const results = [];

    for (const file of req.files) {
      const dataBuffer = fs.readFileSync(file.path);
      const data = await pdfParse(dataBuffer);

      const fileData = {
        title: file.originalname,
        content: data.text,
        category: "pdf-reader",
      };

      results.push(fileData);

      const clearFileData = {
        title: file.originalname,
        content: data.text
          .replace(/\r?\n|\r/g, " ")
          .replace(/\\/g, "\\\\")
          .replace(/"/g, '\\"'),
        category: "pdf-reader",
      };

      // Invia il PDF al webhook
      await axios.post(URL_ADD_PDF_PRODUCTION, clearFileData, {
        headers: { "Content-Type": "application/json" },
      });

      fs.unlinkSync(file.path); // elimina file dopo lettura
    }

    res.send({ files: results });
  } catch (err) {
    console.error(err);
    res
      .status(500)
      .send({ error: "Errore durante la lettura dei PDF o invio al webhook" });
  }
});

app.post("/upload-excel", upload.single("excel"), async (req, res) => {
  if (!req.file) {
    return res.status(400).send({ error: "Nessun file ricevuto" });
  }

  const file = req.file;

  try {
    // Leggi Excel
    const workbook = xlsx.readFile(file.path);
    const firstSheetName = workbook.SheetNames[0];
    const worksheet = workbook.Sheets[firstSheetName];
    const rows = xlsx.utils.sheet_to_json(worksheet, { defval: "" });

    console.log(
      `Processando ${rows.length} righe dal file ${file.originalname}...`
    );

    // Elabora e mappa i dati dall'Excel
    const analyzeResult = analyzeCourses(rows);

    console.log(`Corsi elaborati: ${analyzeResult.length}`);

    // Prepara il payload per il webhook n8n
    // Questo è il formato che si aspetta il nodo Code che abbiamo creato
    const webhookPayload = {
      title: `Analisi Corsi - ${file.originalname}`,
      content: JSON.stringify(analyzeResult), // Array dei corsi serializzato
    };

    // Invia i dati al webhook n8n
    const webhookResponse = await axios.post(
      URL_ADD_EXCEL_TEST,
      webhookPayload,
      {
        headers: {
          "Content-Type": "application/json",
        },
        timeout: 30000, // 30 secondi timeout
      }
    );

    console.log("Risposta dal webhook n8n:", webhookResponse.data);

    // Risposta al client con informazioni complete
    res.status(200).send({
      success: true,
      message: "File Excel elaborato e dati inviati al database",
      file_info: {
        filename: file.originalname,
        sheet_name: firstSheetName,
        total_rows: rows.length,
        processed_courses: analyzeResult.length,
      },
      // Anteprima delle prime 5 righe per l'interfaccia utente
      preview: {
        sheet_name: firstSheetName,
        rows: rows.slice(0, 5),
        headers: rows.length > 0 ? Object.keys(rows[0]) : [],
      },
      // Dati elaborati (primi 5 per non appesantire la risposta)
      processed_data: analyzeResult.slice(0, 5),
      // Risposta dal webhook n8n
      database_response: webhookResponse.data,
    });
  } catch (err) {
    console.error("Errore durante l'elaborazione:", err);

    // Gestisci diversi tipi di errore
    if (err.code === "ECONNREFUSED") {
      return res.status(503).send({
        error:
          "Errore di connessione al webhook n8n. Verifica che n8n sia in esecuzione.",
        details: err.message,
      });
    }

    if (err.response) {
      // Errore dal webhook n8n
      console.error("Errore response dal webhook:", err.response.data);
      return res.status(500).send({
        error: "Errore dal webhook n8n durante l'inserimento nel database",
        webhook_error: err.response.data,
        details: err.message,
      });
    }

    // Errore generico
    res.status(500).send({
      error: "Errore durante l'elaborazione del file Excel",
      details: err.message,
    });
  } finally {
    // Pulisci il file temporaneo
    try {
      fs.unlinkSync(file.path);
      console.log(`File temporaneo eliminato: ${file.path}`);
    } catch (e) {
      console.warn("Impossibile eliminare file temporaneo:", e.message);
    }
  }
});

app.get("/interroga-corsi", async (req, res) => {
  try {
    const webhookResponse = await axios.get(
      "http://localhost:5678/webhook-test/interroga_corsi"
    );
    console.log(webhookResponse.data)
    res.json(webhookResponse.data.response);
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

// Funzione per elaborare e mappare i dati dall'Excel
function analyzeCourses(rows) {
  console.log("Inizio elaborazione corsi...");

  if (!rows || rows.length === 0) {
    console.log("Nessuna riga da elaborare");
    return [];
  }

  // Log delle colonne disponibili nel primo record
  if (rows[0]) {
    console.log("Colonne disponibili nell'Excel:", Object.keys(rows[0]));
  }

  // Mappa le righe Excel ai campi che il webhook n8n si aspetta
  const mapped = rows.map((row, index) => {
    const mappedRow = {
      // Questi sono i 3 campi base che vengono estratti dall'Excel
      // I nomi delle colonne devono corrispondere esattamente a quelli nel tuo Excel
      codice: (
        row["CODICE DEL CORSO"] ||
        row["Codice del Corso"] ||
        row["codice"] ||
        ""
      )
        .toString()
        .trim(),
      descrizione_estesa: (
        row["DESCRIZIONE ESTESA _"] ||
        row["Descrizione Estesa _"] ||
        row["descrizione_estesa"] ||
        ""
      )
        .toString()
        .trim(),
      descrizione_abbreviata: (
        row["DESCRIZIONE ABBREVIATA _"] ||
        row["Descrizione Abbreviata _"] ||
        row["descrizione_abbreviata"] ||
        ""
      )
        .toString()
        .trim(),

      // Campi aggiuntivi se presenti nell'Excel (opzionali)
      denominazione_corso: (
        row["DENOMINAZIONE CORSO"] ||
        row["Denominazione Corso"] ||
        row["denominazione"] ||
        ""
      )
        .toString()
        .trim(),
      area_formazione: (
        row["AREA FORMAZIONE"] ||
        row["Area Formazione"] ||
        row["area"] ||
        ""
      )
        .toString()
        .trim(),
      settore_formazione: (
        row["SETTORE FORMAZIONE"] ||
        row["Settore Formazione"] ||
        row["settore"] ||
        ""
      )
        .toString()
        .trim(),
      tipo_corso: (row["TIPO CORSO"] || row["Tipo Corso"] || row["tipo"] || "")
        .toString()
        .trim(),

      // Metadati per debugging
      _row_index: index + 1,
    };

    // Log ogni 100 record per monitorare il progresso
    if (index % 100 === 0) {
      console.log(`Elaborata riga ${index + 1}/${rows.length}`);
    }

    return mappedRow;
  });

  // Filtra eventuali righe completamente vuote
  const filtered = mapped.filter(
    (row) => row.codice || row.descrizione_estesa || row.descrizione_abbreviata
  );

  console.log(
    `Elaborazione completata: ${filtered.length} corsi validi su ${rows.length} righe totali`
  );

  return filtered;
}

app.listen(port, () => {
  console.log(`Server in ascolto su http://localhost:${port}`);
});
