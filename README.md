# n8n-ollama-rag
Configurazioni di un workflow con n8n - ollama - (un llm qualsiasi) - sqlite per fare RAG
## Configurazione Docker

Avvia i container:
```bash
docker-compose up -d
```
Verifica che i servizi siano attivi:
```bash
docker-compose ps
```
6. Installazione di un modello leggero in Ollama
Accedi al container Ollama:
```bash
docker exec -it ollama bash
```
Installa un modello piccolo (consigliati per test):

 Phi-3 Mini (3.8GB) - ottimo per test
```bash
ollama pull phi3:mini
```
Modello più elevato
```bash
ollama pull gemma3:12b
```


##per embedding scaricare anche
```bash
ollama pull nomic-embed-text
ollama pull mxbai-embed-large:335m
```
### Configurazione di n8n

Apri il browser e vai su http://localhost:5678
Completa la configurazione iniziale di n8n (crea un account admin)

1. Nuovo workflow
2. Clicca su "New workflow"
3. Aggiungi nodo HTTP Request
    Clicca su "+" per aggiungere un nodo
    Cerca "HTTP Request" e selezionalo
    Configura il nodo:

    Method: POST
    URL: http://ollama:11434/api/generate
    Headers:
    Content-Type: application/json
   Body (JSON):
   ```javascript
   {
      "model": "phi3:mini",
      "prompt": "Spiega cos'è l'intelligenza artificiale in 2 frasi",
      "stream": false
   }
   ```
4. Aggiungi nodo Manual Trigger
5. Aggiungi un nodo "Manual Trigger" all'inizio
6.  Collegalo al nodo HTTP Request
7.  Test del workflowì
8.  Salva il workflow
9.  Clicca su "Test workflow"
10.  Clicca su "Execute node" sul Manual Trigger

# Aggiungi il nodo "Set"

Aggiungi un nodo **"Set"** tra i nodi `HTTP Request` e `Respond to Webhook`:

`[Webhook] → [HTTP Request] → [Set] → [Respond to Webhook]`

---

### 2. Configurazione del nodo "Set"

[![Configurazione del Nodo Set](https://img.shields.io/badge/Nodo_Set-blue?logo=nodedotjs)](https://www.google.com)

* **Keep Only Set Fields:** Abilita questa opzione (spunta la casella).
* **Fields to Set:**
    * **Name:** `ai_response`
    * **Value:** `{{ $json[0].response }}` (assicurati che il pulsante `fx` sia attivato).

### Aggiungere Nodo "MySQL" per ricerca documenti
    Configurazione nodo MySQL:

    Aggiungi il nodo "MySQL" dopo il Webhook
    Credentials: Crea nuove credenziali:
    
    Host: mysql_rag
    Database: knowledge_base
    User: n8n_user
    Password: n8n_password123
    Port: 3306
    
    
    Operation: Execute Query
    Query (con fx attivato):
    
```sql
USE knowledge_base;

CREATE TABLE documents (
    id INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    category VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO documents (title, content, category) VALUES 
('Installazione Docker', 'Per installare Docker, scarica Docker Desktop dal sito ufficiale docker.com. Scegli la versione per il tuo sistema operativo e segui la procedura di installazione guidata.', 'docker'),

('Comandi Docker Base', 'I comandi Docker essenziali sono: docker run per eseguire un container, docker build per costruire immagini, docker ps per vedere i container attivi, docker stop per fermare un container.', 'docker'),

('Docker Compose', 'Docker Compose permette di gestire applicazioni multi-container. Usa il file docker-compose.yml per definire i servizi e docker-compose up per avviare tutto insieme.', 'docker'),

('Comandi Git', 'I comandi Git principali sono: git init per inizializzare un repo, git add per staggiare modifiche, git commit per salvare modifiche, git push per caricare su repository remoto.', 'git'),

('Branching Git', 'Git permette di creare branch con git branch nome-branch, switchare con git checkout nome-branch, e unire con git merge. Questo permette sviluppo parallelo.', 'git'),

('n8n Workflow', 'n8n è uno strumento di automazione workflow. Permette di collegare diversi servizi tramite nodi grafici e creare automazioni complesse senza scrivere codice.', 'n8n');
```

```sql
SELECT title, content, category 
FROM documents 
WHERE content LIKE CONCAT('%', ?, '%') 
   OR title LIKE CONCAT('%', ?, '%')
   OR category LIKE CONCAT('%', ?, '%')
ORDER BY 
  CASE 
    WHEN title LIKE CONCAT('%', ?, '%') THEN 1 
    ELSE 2 
  END
LIMIT 3
```

### Nodo "Set" per preparare il contesto RAG
```sql
SELECT content, title, relevance_score 
FROM documents 
WHERE content LIKE '%{{ $json.body.message }}%' 
OR title LIKE '%{{ $json.body.message }}%'
LIMIT 3
```

    Nome: rag_context
    Value (con fx):
    javascript// Combina i risultati della ricerca
```javascript
    const searchResults = $json.map(doc => 
      `Documento: ${doc.title}\nContenuto: ${doc.content}`
    ).join('\n\n');
    const userQuestion = $node['Webhook'].json.body.message;
    return `Contesto da base di conoscenza:
    ${searchResults}
```
Domanda dell'utente: ${userQuestion}

Istruzioni: Rispondi alla domanda usando SOLO le informazioni nel contesto sopra. Se le informazioni non sono sufficienti, dillo chiaramente.`;
### Modifica il nodo HTTP Request (Ollama)
JSON Body:
```javascript
json{
  "model": "llama3.2:1b",
  "prompt": "{{ $json.rag_context }}",
  "stream": false
```
