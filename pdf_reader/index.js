const express = require("express");
const multer = require("multer");
const pdfParse = require("pdf-parse");
const fs = require("fs");
const axios = require("axios");

const app = express();
const port = 3000;

// Configurazione multer per upload multiplo
const upload = multer({ dest: "uploads/" });

// Middleware per servire file statici
app.use(express.static("public"));

// Endpoint per caricare più PDF
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
          .replace(/\r?\n|\r/g, " ") // rimuove i ritorni a capo
          .replace(/\\/g, "\\\\") // scappa eventuali backslash già presenti
          .replace(/"/g, '\\"'), // scappa tutte le virgolette doppie
        category: "pdf-reader",
      };

      // Invia il PDF al webhook
      await axios.post(
        "http://localhost:5678/webhook-test/add-document",
        JSON.stringify(clearFileData),
        {
          headers: { "Content-Type": "application/json" },
        }
      );

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

app.listen(port, () => {
  console.log(`Server in ascolto su http://localhost:${port}`);
});
