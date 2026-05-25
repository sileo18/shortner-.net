# URL Shortener API

## Visão Geral
Este README documenta todos os endpoints da API de encurtador de URLs e mostra exemplos de teste usando `curl`.

- Prefixo base da API: `/api`
- Endpoint público de redirecionamento: `/{shortCode}`
- O serviço gera URLs curtas e redireciona para a URL original
- `AppSettings.BaseUrl` define a URL de retorno gerada em `POST /api/url/shorten`

---

## 1) Criar Usuário
- Endpoint: `POST /api/users`
- Descrição: cria um usuário e retorna o `userId`

### Request
```json
{
  "username": "sileo"
}
```

### Response (201 Created)
```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "username": "sileo"
}
```

### curl
```bash
curl -X POST http://<host>:<port>/api/users \
  -H "Content-Type: application/json" \
  -d '{"username":"sileo"}'
```

---

## 2) Buscar Usuário
- Endpoint: `GET /api/users/{userId}`
- Descrição: retorna os dados do usuário

### curl
```bash
curl http://<host>:<port>/api/users/00000000-0000-0000-0000-000000000000
```

### Response (200 OK)
```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "username": "sileo"
}
```

### Erros
- `404 Not Found` se o usuário não existir

---

## 3) Encurtar URL
- Endpoint: `POST /api/url/shorten`
- Descrição: cria um short code para uma URL original pertencente a um usuário

### Request
```json
{
  "userId": "00000000-0000-0000-0000-000000000000",
  "originalUrl": "https://www.google.com",
  "userPrefix": "sileo"
}
```

### Response (200 OK)
```json
{
  "shortCode": "sileo_abc123",
  "shareableUrl": "https://seu-encurtador.com.br/sileo_abc123"
}
```

### curl
```bash
curl -X POST http://<host>:<port>/api/url/shorten \
  -H "Content-Type: application/json" \
  -d '{
    "userId":"00000000-0000-0000-0000-000000000000",
    "originalUrl":"https://www.google.com",
    "userPrefix":"sileo"
  }'
```

### Erros
- `400 Bad Request` se `originalUrl` estiver vazio
- `404 Not Found` se `userId` não existir

---

## 4) Redirecionar Short URL
- Endpoint: `GET /{shortCode}`
- Descrição: redireciona para a URL original

### curl
```bash
curl -L http://<host>:<port>/sileo_abc123
```

### Comportamento
- Retorna `302 Found` com `Location` apontando para a URL original
- Retorna `404 Not Found` se o short code não existir

---

## 5) Detalhes da URL
- Endpoint: `GET /api/url/{shortCode}/details`
- Descrição: retorna meta informações da URL encurtada

### curl
```bash
curl http://<host>:<port>/api/url/sileo_abc123/details
```

### Response (200 OK)
```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "originalUrl": "https://www.google.com",
  "createdAt": "2026-05-25T21:00:00Z",
  "expiresAt": null,
  "clickCount": 5
}
```

### Erros
- `404 Not Found` se o short code não existir

---

## 6) Contagem de Cliques
- Endpoint: `GET /api/url/{shortCode}/clicks`
- Descrição: retorna o número de cliques do short code

### curl
```bash
curl http://<host>:<port>/api/url/sileo_abc123/clicks
```

### Response (200 OK)
```json
{
  "shortCode": "sileo_abc123",
  "clickCount": 5
}
```

---

## 7) URLs do Usuário
- Endpoint: `GET /api/user/{userId}/urls`
- Descrição: retorna todas as URLs do usuário

### curl
```bash
curl http://<host>:<port>/api/user/00000000-0000-0000-0000-000000000000/urls
```

### Erros
- `404 Not Found` se o usuário não existir

---

## 8) Códigos de Link do Usuário
- Endpoint: `GET /api/user/{userId}/link-codes`
- Descrição: lista todos os short codes do usuário

### curl
```bash
curl http://<host>:<port>/api/user/00000000-0000-0000-0000-000000000000/link-codes
```

### Erros
- `404 Not Found` se o usuário não existir

---

## Notas
- O endpoint de redirecionamento público não usa `/api`.
- O `shareableUrl` retornado por `POST /api/url/shorten` é construído usando `AppSettings.BaseUrl`.
- Se você expor a API em porta 80, use `http://<host>` como `BaseUrl`.
- Em produção, use um domínio e HTTPS.

---

## Fluxo de Teste Rápido
1. `POST /api/users` → cria usuário
2. `POST /api/url/shorten` → gera `shortCode`
3. `GET /{shortCode}` → redireciona para a URL original
4. `GET /api/url/{shortCode}/clicks` → obtém contagem de cliques
