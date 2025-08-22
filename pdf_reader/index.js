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

app.use(express.json()); // Per JSON
app.use(express.urlencoded({ extended: true })); // Per form data

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

app.post("/interroga-ai", async (req, res) => {
  try {
    const webhookResponse = await axios.post(
      "http://localhost:5678/webhook-test/rag-query",
      { question: req.body.domanda }
    );
    res.json({ response: webhookResponse.data });
  } catch (err) {
    res.status(500).json({ error: err.message });
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

    // Elabora e mappa i dati dall'Excel
    const analyzeResult = analyzeCourses(rows);
    // Prepara il payload per il webhook n8n
    // Questo è il formato che si aspetta il nodo Code che abbiamo creato
    const webhookPayload = {
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
      }
    );

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
    res.status(500).send({
      error: "Errore durante l'elaborazione del file Excel",
      details: err.message,
    });
  } finally {
    fs.unlinkSync(file.path);
  }
});

app.get("/interroga-corsi", async (req, res) => {
  try {
    const webhookResponse = await axios.get(
      "http://localhost:5678/webhook-test/interroga_corsi"
    );
    res.json(webhookResponse.data.response);
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

function analyzeCourses(rows) {
  if (!rows || rows.length === 0) return [];

  // Prende l'ordine delle colonne dal primo oggetto (header order)
  const headers = Object.keys(rows[0]);

  // Helper: normalizza stringa
  const norm = (v) => (v === null || v === undefined ? "" : String(v).trim());

  // Helper: parse dd/mm/yyyy -> YYYY-MM-DD (ritorna stringa vuota se non valida)
  const parseDateDMY = (s) => {
    s = norm(s);
    if (!s) return "";
    const parts = s.split(/[\/\.\-]/).map((p) => p.trim());
    if (parts.length !== 3) return "";
    let [d, m, y] = parts;
    if (y.length === 2) y = "20" + y;
    if (!/^\d+$/.test(d) || !/^\d+$/.test(m) || !/^\d+$/.test(y)) return "";
    d = d.padStart(2, "0");
    m = m.padStart(2, "0");
    y = y.padStart(4, "0");
    return `${y}-${m}-${d}`;
  };

  // Mappatura: values[i] corrisponde alla colonna i-esima nell'ordine headers
  const mapped = rows.map((row, index) => {
    // ottieni i valori rispettando l'ordine delle colonne
    const values = headers.map((h) => row[h]);

    // ESEMPIO di mapping per indici (adatta gli indici se il tuo ordine è diverso):
    const mappedRow = {
      index,
      // indice 0 -> 'CODICE DEL CORSO'
      codice: norm(values[0]),
      // indice 1 -> 'DENOMINAZIONE ATTUALE CORSO'
      denominazione_attuale: norm(values[1]),
      // indice 2 -> 'DESCRIZIONE ABBREVIATA '
      descrizione_abbreviata: norm(values[2]),
      // indice 3 -> 'DESCRIZIONE ESTESA '
      descrizione_estesa: norm(values[3]),
      // indice 4 -> 'DATA ISTITUZIONE' (normalizzata ISO)
      data_istituzione: parseDateDMY(values[4]),
      // indice 5 -> 'Motivo Ist.'
      motivo_istituzione: norm(values[5]),
      // indice 6 -> 'SOSPESO / SOPPRESSO'
      sospeso: norm(values[6]),
      // indice 7 -> 'DATA SOPPRESSIONE / SOPENSIONE'
      data_soppressione: parseDateDMY(values[7]),
      // indice 8 -> 'MOTIVO SOPPRESSIONE / SOSPENSIONE'
      motivo_soppressione: norm(values[8]),
      // indice 9 -> 'AREA FORMAZIONE'
      area_formazione: norm(values[9]),
      // indice 10 -> 'Settore Perseo' (o SETTORE_FORM)
      settore_perseo: norm(values[10]),
      // indice 11 -> 'SETTORE FORMAZIONE '
      settore_formazione: norm(values[11]),
      // indice 12 -> 'TIPO CORSO'
      tipo_corso: norm(values[12]),
      // indice 13 -> 'DENOMINAZIONE TITOLO'
      denominazione_titolo: norm(values[13]),
      // aggiungi altri campi se servono: values[14], values[15], ...
      _raw_values: values, // opzionale: per debug
    };
    return mappedRow;
  });

  return mapped;
}

app.listen(port, () => {
  console.log(`Server in ascolto su http://localhost:${port}`);
});
