# ATM Projesi — C# .NET

Bir banka ATM sistemini simüle eden, **ASP.NET Core Web API** backend, **Blazor Server** web arayüzü ve **Spectre.Console** terminal istemcisinden oluşan tam katmanlı bir .NET uygulaması.

## Özellikler

- JWT tabanlı kimlik doğrulama (Kart No + PIN)
- 3 hatalı girişte kart kilitleme
- Para yatırma / çekme / havale (EFT)
- İşlem geçmişi
- SQLite veritabanı (EF Core)
- Gerçekçi ATM simülasyonu web arayüzü (Blazor Server)
- Renkli terminal arayüzü (Spectre.Console)
- API dokümantasyonu (Scalar UI)

## Teknolojiler

| Katman | Teknoloji |
|---|---|
| Backend | ASP.NET Core 10 Web API |
| Web UI | Blazor Server (Interactive Server) |
| ORM | Entity Framework Core 10 + SQLite |
| Kimlik Doğrulama | JWT Bearer Token + BCrypt |
| Terminal UI | Spectre.Console |
| API Dokümantasyonu | Scalar |

## Proje Yapısı

```
ATM.sln
├── src/
│   ├── ATM.Core/            # Entity'ler, DTO'lar, Interface'ler
│   ├── ATM.Infrastructure/  # EF Core, Repository'ler, Veritabanı
│   ├── ATM.API/             # Web API, Controller'lar, JWT
│   ├── ATM.ConsoleClient/   # Terminal ATM ekranı (Spectre.Console)
│   └── ATM.Web/             # Blazor Server ATM simülasyonu
```

## Gereksinimler

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Kurulum ve Çalıştırma

```bash
git clone https://github.com/silakarahan01/atm-dotnet.git
cd atm-dotnet
```

---

### Seçenek 1 — Web Arayüzü (Blazor)

Tek başına çalışır, ayrıca API başlatmaya gerek yoktur.

```bash
cd src/ATM.Web
dotnet run
```

Tarayıcıda aç: `http://localhost:5227`

**Ekranlar:**
- Karşılama → Kart numarası girişi
- PIN → 4 haneli tuş takımı (klavye desteği)
- Ana Menü → 5 işlem seçeneği
- Bakiye Sorgulama → animasyonlu sayaç
- Para Çekme → hızlı tutar butonları veya özel tutar
- Para Yatırma → onaylı yatırma akışı
- Havale / EFT → hedef hesap + tutar + onay özeti
- İşlem Geçmişi → son 10 işlem

**Öne çıkan detaylar:**
- Banka odası arkaplanı (duvar/zemin gradyanı)
- 3 boyutlu ATM makine gövdesi (CSS box-shadow)
- Kart takma ve para çıkış animasyonları
- Para çekiminde 5 banknotun slottan kademeli çıkışı
- 60 saniyelik oturum sayacı (uyarı renkleriyle)
- Bootstrap Icons

---

### Seçenek 2 — Terminal İstemcisi (Spectre.Console)

#### 1. API'yi başlat

```bash
cd src/ATM.API
dotnet run
```

İlk çalıştırmada veritabanı ve test kullanıcıları otomatik oluşturulur.  
Scalar arayüzü: `http://localhost:5169/scalar/v1`

#### 2. Console Client'ı başlat (yeni terminal)

```bash
cd src/ATM.ConsoleClient
dotnet run
```

---

## Test Kullanıcıları

| Ad | Kart Numarası | PIN | Hesap No | Bakiye |
|---|---|---|---|---|
| Ahmet Yılmaz | `1234567890123456` | `1234` | TR001234567890 | 5.000 TL |
| Fatma Kaya | `6543210987654321` | `5678` | TR009876543210 | 1.000 TL |

---

## API Endpoint'leri

| Method | Route | Açıklama | Auth |
|---|---|---|---|
| POST | `/api/auth/login` | Kart + PIN ile giriş, JWT döner | Hayır |
| PUT | `/api/auth/change-pin` | PIN değiştirme | Evet |
| GET | `/api/account/balance` | Bakiye sorgulama | Evet |
| GET | `/api/account/info` | Hesap bilgileri | Evet |
| POST | `/api/transaction/deposit` | Para yatırma | Evet |
| POST | `/api/transaction/withdraw` | Para çekme | Evet |
| POST | `/api/transaction/transfer` | Hesaplar arası transfer | Evet |
| GET | `/api/transaction/history` | İşlem geçmişi | Evet |

## Mimari

```
[ATM.Web]          [ATM.ConsoleClient]
 Blazor Server       Spectre.Console
     │                     │ HTTP
     │               [ATM.API]
     │             Controller → Service → Repository
     │                     │
     └──────── [ATM.Infrastructure] ────────────────
                    EF Core → SQLite
                         │
                    [ATM.Core]
               Entity / DTO / Interface
```

- **ATM.Core**: Sıfır dış bağımlılık — sadece domain modelleri
- **ATM.Infrastructure**: Veritabanı işlemleri, EF Core, seed
- **ATM.API**: HTTP katmanı, JWT, iş mantığı servisleri
- **ATM.Web**: Blazor Server, ATMStateService, tam UI simülasyonu
- **ATM.ConsoleClient**: Terminal arayüzü, API'ye HTTP ile bağlanır
