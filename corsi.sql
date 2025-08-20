-- Dump della struttura di tabella public.corsi
DROP TABLE IF EXISTS "corsi";
CREATE TABLE IF NOT EXISTS "corsi" (
	"id" SERIAL NOT NULL,
	"codice_corso" VARCHAR(50) NULL DEFAULT NULL,
	"denominazione_attuale_corso" TEXT NULL DEFAULT NULL,
	"descrizione_abbreviata" TEXT NULL DEFAULT NULL,
	"descrizione_estesa" TEXT NULL DEFAULT NULL,
	"data_istituzione" DATE NULL DEFAULT NULL,
	"motivo_istituzione" TEXT NULL DEFAULT NULL,
	"sospeso_soppresso" BOOLEAN NULL DEFAULT NULL,
	"data_soppressione" DATE NULL DEFAULT NULL,
	"motivo_soppressione" TEXT NULL DEFAULT NULL,
	"area_formazione" TEXT NULL DEFAULT NULL,
	"settore_formazione" TEXT NULL DEFAULT NULL,
	"tipo_corso" TEXT NULL DEFAULT NULL,
	"denominazione_titolo" TEXT NULL DEFAULT NULL,
	"codice_titolo" VARCHAR(50) NULL DEFAULT NULL,
	"brevetto_associato_corso" TEXT NULL DEFAULT NULL,
	"ente_programmatore" TEXT NULL DEFAULT NULL,
	"ente_erogatore" TEXT NULL DEFAULT NULL,
	"durata_gg_add" INTEGER NULL DEFAULT NULL,
	"frequentatori" TEXT NULL DEFAULT NULL,
	"job_description" TEXT NULL DEFAULT NULL,
	"posizione_prevista" TEXT NULL DEFAULT NULL,
	"modalita_selezione" TEXT NULL DEFAULT NULL,
	"presente_in_n8" BOOLEAN NULL DEFAULT NULL,
	"parole_chiave" TEXT NULL DEFAULT NULL,
	"corso_da_bonificare" BOOLEAN NULL DEFAULT NULL,
	"corso_da_lasciare" BOOLEAN NULL DEFAULT NULL,
	"corso_da_aggiungere" BOOLEAN NULL DEFAULT NULL,
	"codici___associati" TEXT NULL DEFAULT NULL,
	PRIMARY KEY ("id")
);

-- L’esportazione dei dati non era selezionata.

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
