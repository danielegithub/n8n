-- phpMyAdmin SQL Dump
-- version 5.2.2
-- https://www.phpmyadmin.net/
--
-- Host: mysql
-- Creato il: Ago 18, 2025 alle 18:46
-- Versione del server: 8.0.43
-- Versione PHP: 8.2.29

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `rag_database`
--

-- --------------------------------------------------------

--
-- Struttura della tabella `documents`
--

CREATE TABLE `documents` (
  `id` int NOT NULL,
  `title` varchar(255) DEFAULT NULL,
  `original_title` varchar(255) DEFAULT NULL,
  `content` longtext,
  `category` varchar(100) DEFAULT 'generale',
  `model` varchar(100) DEFAULT NULL,
  `embedding_vector` json DEFAULT NULL,
  `chunk_index` int DEFAULT '0',
  `total_chunks` int DEFAULT '1',
  `is_chunked` tinyint(1) DEFAULT '0',
  `file_name` varchar(255) DEFAULT NULL,
  `file_path` text,
  `mime_type` varchar(100) DEFAULT NULL,
  `language` varchar(8) DEFAULT NULL,
  `doc_length` int DEFAULT NULL,
  `metadata` json DEFAULT NULL,
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Indici per le tabelle scaricate
--

--
-- Indici per le tabelle `documents`
--
ALTER TABLE `documents`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_original_title` (`original_title`),
  ADD KEY `idx_chunk` (`chunk_index`);
ALTER TABLE `documents` ADD FULLTEXT KEY `ft_title_content` (`title`,`content`);

--
-- AUTO_INCREMENT per le tabelle scaricate
--

--
-- AUTO_INCREMENT per la tabella `documents`
--
ALTER TABLE `documents`
  MODIFY `id` int NOT NULL AUTO_INCREMENT;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
