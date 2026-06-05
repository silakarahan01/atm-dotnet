# ATM Projesi — Clean Architecture & CQRS (.NET 10)

Bir banka ATM sistemini simüle eden, **Clean Architecture** ve **CQRS** desenleriyle kurgulanmış tam katmanlı bir .NET çözümü. Backend **ASP.NET Core Web API**, web arayüzü **Blazor Server**, terminal istemcisi **Spectre.Console** ile yazılmıştır. Veriler **PostgreSQL** üzerinde **EF Core** ile tutulur; çözüm **Docker** ile çalışır ve **birim + entegrasyon testleriyle** kapsanmıştır.

## Öne Çıkan Mimari Özellikler

- **Clean Architecture** — Domain / Application / Infrastructure / Presentation katmanları, bağımlılıklar içe doğru akar.
- **CQRS (MediatR)** — Her işlem ayrı bir `Command`/`Query` + `Handler`; ince controller'lar yalnızca `mediator.Send(...)` çağırır.
- **Tek iş-mantığı kaynağı** — Hem Web API hem Blazor arayüzü aynı MediatR handler'larını kullanır; kod tekrarı yoktur.
- **Result deseni** — İş kuralı hataları exception yerine `Result`/`Error` ile taşınır; akış öngörülebilir.
- **Davranışlı domain** — Bakiye ve kart bloke kuralları entity'lerin içindedir (`Account.Withdraw`, `Card.RegisterFailedAttempt`).
- **MediatR pipeline behavior'ları** — Otomatik `FluentValidation` doğrulaması ve loglama, cross-cutting olarak tek yerde.
- **Atomik transfer** — `IUnitOfWork` ile kaynak ve hedef hesap güncellemesi tek transaction'da; para "yolda kaybolmaz".
- **RFC 7807 ProblemDetails** — Tutarlı, standart hata yanıtları; `IExceptionHandler` ile beklenmeyen hatalar.
- **Test kapsamı** — xUnit birim testleri (domain + handler) ve Testcontainers ile gerçek PostgreSQL'e karşı uçtan uca entegrasyon testleri.

## Teknolojiler

| Alan | Teknoloji |
|---|---|
| Backend | ASP.NET Core 10 Web API |
| Mimari | Clean Architecture + CQRS (MediatR) |
| Doğrulama | FluentValidation (pipeline behavior) |
| Web UI | Blazor Server (Interactive Server) |
| Terminal UI | Spectre.Console |
| ORM / Veritabanı | Entity Framework Core 10 + PostgreSQL |
| Kimlik Doğrulama | JWT Bearer Token + BCrypt |
| Konteyner | Docker + Docker Compose |
| API Dokümantasyonu | OpenAPI + Scalar |
| Test | xUnit, FluentAssertions, NSubstitute, Testcontainers, WebApplicationFactory |

## Proje Yapısı

```
ATM.slnx
├── src/
│   ├── ATM.Domain/          # Entity'ler (davranışlı), Enum'lar, Result/Error, domain hataları — sıfır bağımlılık
│   ├── ATM.Application/      # CQRS Command/Query + Handler, FluentValidation, pipeline behavior'lar, soyutlamalar
│   ├── ATM.Infrastructure/   # EF Core (PostgreSQL), Repository'ler, UnitOfWork, JWT/BCrypt, migration, seed
│   ├── ATM.API/             # İnce controller'lar, JWT, ProblemDetails
│   ├── ATM.Web/             # Blazor Server ATM simülasyonu (handler'ları doğrudan kullanır)
│   └── ATM.ConsoleClient/   # Terminal ATM ekranı (API'ye HTTP ile bağlanır)
└── tests/
    ├── ATM.Domain.UnitTests/        # Domain iş kuralları
    ├── ATM.Application.UnitTests/    # Handler ve validator testleri (NSubstitute)
    └── ATM.API.IntegrationTests/     # Testcontainers + WebApplicationFactory ile uçtan uca
```

### Bağımlılık Akışı

```
ATM.Web ─┐
ATM.API ─┼──► ATM.Application ──► ATM.Domain
         │           ▲
ATM.Infrastructure ──┘  (Application'ın soyutlamalarını uygular)
```

## Gereksinimler

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (PostgreSQL ve testler için)

## Kurulum ve Çalıştırma

```bash
git clone https://github.com/silakarahan01/atm-dotnet.git
cd atm-dotnet
```

### 1. PostgreSQL'i başlat (Docker)

```bash
docker compose up -d db
```

İlk açılışta uygulama veritabanını otomatik migrate eder ve test kullanıcılarını oluşturur.

### 2a. Web Arayüzü (Blazor)

```bash
dotnet run --project src/ATM.Web
```

Tarayıcıda aç: `http://localhost:5227`

### 2b. API + Terminal İstemcisi

```bash
# 1. terminal — API
dotnet run --project src/ATM.API
#   Scalar arayüzü: http://localhost:5169/scalar/v1

# 2. terminal — Console Client
dotnet run --project src/ATM.ConsoleClient
```

### Alternatif: Her şeyi Docker ile çalıştır

```bash
docker compose up --build
```

API: `http://localhost:5169` · PostgreSQL: `localhost:5432`

## Testleri Çalıştırma

```bash
dotnet test
```

> Entegrasyon testleri, Testcontainers aracılığıyla geçici bir PostgreSQL konteyneri ayağa kaldırır; bunun için Docker'ın çalışıyor olması gerekir.

## Test Kullanıcıları

| Ad | Kart Numarası | PIN | Hesap No | Bakiye |
|---|---|---|---|---|
| Ahmet Yılmaz | `1234567890123456` | `1234` | TR001234567890 | 5.000 TL |
| Fatma Kaya | `6543210987654321` | `5678` | TR009876543210 | 1.000 TL |

## API Endpoint'leri

| Method | Route | Açıklama | Auth |
|---|---|---|---|
| POST | `/api/auth/login` | Kart + PIN ile giriş, JWT döner | Hayır |
| PUT | `/api/auth/change-pin` | PIN değiştirme | Evet |
| GET | `/api/account/balance` | Bakiye sorgulama | Evet |
| GET | `/api/account/info` | Hesap bilgileri | Evet |
| POST | `/api/transaction/deposit` | Para yatırma | Evet |
| POST | `/api/transaction/withdraw` | Para çekme | Evet |
| POST | `/api/transaction/transfer` | Hesaplar arası transfer (atomik) | Evet |
| GET | `/api/transaction/history` | İşlem geçmişi | Evet |

İş kuralı hataları (ör. yetersiz bakiye) `400`, kimlik/bloke hataları `401`, bulunamayan kayıtlar `404` ile **ProblemDetails** biçiminde döner.

## İstek Akışı (CQRS)

```
HTTP İsteği
   │
[Controller]  ── mediator.Send(Command/Query) ──►
   │
[LoggingBehavior] ► [ValidationBehavior] ► [Handler]
                                              │
                          [Repository] + [UnitOfWork] (EF Core / PostgreSQL)
                                              │
                                       Result<T> ◄── Domain (iş kuralları)
   │
ProblemDetails / 200 OK ◄──────────────────────┘
```
