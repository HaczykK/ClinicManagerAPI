# 🏥 Projekt zaliczeniowy – ASP.NET Core
## **System zarządzania przychodnią medyczną 2.0**

[![Build Status](https://github.com/HaczykK/ClinicManagerAPI/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/HaczykK/ClinicManagerAPI/actions/workflows/dotnet-ci.yml)

---

## 🧠 **Cel projektu**

Zaprojektuj i zaimplementuj aplikację webową do obsługi przychodni medycznej. Aplikacja powinna umożliwiać:

- zarządzanie pacjentami i ich dokumentacją medyczną,
- rejestrację wizyt z procedurami i lekami,
- przypisywanie wizyt lekarzom,
- prowadzenie notatek klinicznych do wizyt,
- generowanie raportów PDF (np. karta wizyty, recepta),
- dodawanie zdjęć/skanów dokumentów do kartoteki pacjenta,
- filtrowanie i raportowanie udzielonych świadczeń.

Projekt powinien mieć przejrzystą strukturę, modularność, oraz używać nowoczesnych narzędzi: **EF Core**, **Dependency Injection**, **Mapperly**, **Identity**, **OpenAPI**, **Razor Pages, MVC (lub frontend SPA)**.

---

## 🧑‍🤝‍🧑 Zespół

- Zespół: **2 osoby**
- Praca nad repozytorium GitHub (wymagana historia commitów)

---

## 🚀 **Uruchomienie lokalne**

### Wymagania

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (lokalnie: LocalDB)

### Kroki

```bash
dotnet restore ClinicManagerAPI.slnx
dotnet build ClinicManagerAPI.slnx
dotnet run --project ClinicManagerAPI/ClinicManagerAPI.csproj
```

Aplikacja startuje pod adresem: `http://localhost:5214`

### Konta testowe (seed)

| Rola | E-mail | Hasło |
|------|--------|-------|
| Admin | `admin@clinic.pl` | `Admin123!` |
| Lekarz | `lekarz@clinic.pl` | `Lekarz123!` |
| Rejestratorka | `rejestratorka@clinic.pl` | `Rejestratorka123!` |

Role: `Admin`, `Lekarz`, `Rejestratorka` — tworzone automatycznie przy starcie aplikacji.

---

## ✅ **Wymagania funkcjonalne**

| Moduł                 | Wymagania                                                                 |
|-----------------------|---------------------------------------------------------------------------|
| 🔐 Uwierzytelnianie   | Rejestracja, logowanie (ASP.NET Identity), role: `Admin`, `Lekarz`, `Rejestratorka` |
| 👤 Pacjenci           | CRUD pacjentów, wyszukiwanie (po nazwisku/PESEL), lista wizyt pacjenta   |
| 🪪 Kartoteka          | CRUD kartoteki, upload skanu/zdjęcia dokumentu (np. skierowanie), PESEL, nr ubezpieczenia |
| 📅 Wizyty             | Tworzenie wizyt, statusy (zaplanowana/w trakcie/zakończona/anulowana), przypisywanie lekarza |
| 🩺 Procedury          | Lista procedur medycznych: opis + koszt świadczenia                      |
| 💊 Leki / recepty     | Wybór leków z katalogu, dawkowanie, ilość, koszt                         |
| 📝 Notatki kliniczne  | Notatki wewnętrzne do wizyty (wywiad, rozpoznanie, zalecenia)            |
| 📦 Katalog leków      | CRUD leków, tylko dla `Admin` / `Rejestratorka`                          |
| 📈 Raporty            | Koszt świadczeń danego pacjenta / lekarza / miesiąca + eksport do PDF    |

---

## ✅ **Pozostałe wymagania**

### 🧩 **1. Indeksy – optymalizacja zapytań**

#### 📌 Zadanie:
- Zidentyfikuj **co najmniej dwa zapytania SELECT**, które są często wykonywane i mają **WHERE** lub **JOIN** po kolumnie niekluczowej (np. wyszukiwanie pacjenta po PESEL, lista wizyt lekarza w danym dniu).
- Dodaj **indeksy nieklastrowane (non-clustered)** do wybranych kolumn.
- Zrób analizę wydajności:
  - **Zrzut planu zapytania (Query Plan)** przed i po dodaniu indeksu.
  - Krótkie porównanie (np. liczba odczytów, operacje przeszukiwania vs seek).
  - Umieść to w **raporcie PDF** z opisem + screenshotami.

#### 📎 Plik: `raport-indeksy.pdf`

---

### 📡 **2. SQL Profiler – nasłuch endpointu**

#### 📌 Zadanie:
- Uruchom **SQL Server Profiler (lub EF Core Logging)**.
- Wybierz konkretny **endpoint API** (np. `GET /api/visits/today`).
- Uruchom aplikację → wywołaj endpoint → zrób screenshot z Profilerem pokazującym zapytanie.
- Dodaj screenshoty + opis działania zapytania + krótki komentarz.

#### 📎 Plik: `raport-sql-profiler.pdf`

---

### ⚙️ **3. GitHub Actions – CI/CD**

Workflow [`.github/workflows/dotnet-ci.yml`](.github/workflows/dotnet-ci.yml) uruchamia się automatycznie przy **push** i **pull request** do brancha `main`.

#### Job `build-and-test`

| Krok | Opis |
|------|------|
| Checkout | Pobranie kodu z repozytorium |
| Setup .NET 10 SDK | Instalacja SDK `10.0.x` na `ubuntu-latest` |
| Restore | `dotnet restore ClinicManagerAPI.slnx` |
| Build | `dotnet build ClinicManagerAPI.slnx --no-restore --configuration Release` |
| Test | `dotnet test` — uruchamiany tylko gdy istnieje projekt testowy (`*Tests.csproj` / `*Test.csproj`) |

#### Job `docker-build`

Po pomyślnym buildzie uruchamiany jest build obrazu Docker:

```bash
docker build -t clinic-manager-api:latest .
```

Status builda widoczny jest w badge na górze tego pliku oraz w zakładce **Actions** na GitHubie.

---

### 📝 **4. Logowanie błędów – NLog**

#### 📌 Zadanie:
- Skonfiguruj **NLog** do logowania wyjątków i zdarzeń:
  - logi zapisywane do pliku (np. `/logs/errors.log`)
  - logowanie błędów kontrolerów i serwisów
  - obsługa logowania przez DI (`ILogger<T>`)

---

### 📤 **5. BackgroundService – raport e-mail**

#### 📌 Zadanie:
- Zaimplementuj usługę w tle (`BackgroundService`), która:
  - raz dziennie (lub co 1–2 minuty dla testów) generuje raport z wizyt zaplanowanych na kolejny dzień
  - zapisuje go jako PDF (np. `upcoming_visits.pdf`)
  - wysyła jako załącznik na e-mail administratora przychodni (np. za pomocą SMTP)

#### 📎 Plik: `raport-nadchodzace-wizyty.pdf`
#### 📎 Klasa: `UpcomingVisitsReportBackgroundService.cs`

---

### 🚀 **6. NBomber – testy wydajności**

#### 📌 Zadanie:
- **Dodaj dodatkowy endpoint API** dedykowany do testów wydajnościowych (np. `GET /api/visits/active`, `GET /api/patients/search?query=...` lub inny endpoint zwracający dane z bazy z JOIN-ami / filtrowaniem).
- Endpoint powinien być udokumentowany w OpenAPI i zwracać realistyczne dane (np. listę aktywnych wizyt z danymi pacjenta i lekarza).
- Skonfiguruj **NBomber** do przetestowania **właśnie tego endpointu**.
- Uruchom test z 50 równoległymi użytkownikami, 100 żądaniami.
- Zapisz **raport PDF z wynikami testu** (czas odpowiedzi, throughput, błędy).

#### Uruchomienie testów wydajnościowych

1. Uruchom API (z seedem danych):
   ```bash
   dotnet run --project ClinicManagerAPI/ClinicManagerAPI.csproj
   ```
2. W drugim terminalu uruchom test NBomber:
   ```bash
   dotnet run --project ClinicManagerAPI.PerformanceTests/ClinicManagerAPI.PerformanceTests.csproj
   ```
3. Raporty (HTML, CSV, TXT) zostaną zapisane w folderze `ClinicManagerAPI.PerformanceTests/reports/`.
4. Domyślny adres API: `http://localhost:5214`. Przy innym porcie ustaw zmienną środowiskową:
   ```bash
   # PowerShell
   $env:API_BASE_URL = "http://localhost:5214"
   dotnet run --project ClinicManagerAPI.PerformanceTests/ClinicManagerAPI.PerformanceTests.csproj
   ```

> Endpoint `GET /api/visits/active` jest bez autoryzacji — wyłącznie do testów wydajnościowych. Nie używaj go w produkcji bez zabezpieczenia.

#### 📎 Plik: `nbomber-report.pdf`
#### 📎 Kod testu: `ClinicManagerAPI.PerformanceTests/VisitsLoadTest.cs`
#### 📎 Kod endpointu: `Controllers/VisitsController.cs`

---

## 🐳 **Docker**

### Build obrazu

```bash
docker build -t clinic-manager-api:latest .
```

### Uruchomienie kontenera

```bash
docker run -d -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Database=ClinicManagerDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True" \
  --name clinic-manager-api clinic-manager-api:latest
```

API dostępne pod adresem: `http://localhost:8080`

> W środowisku produkcyjnym nadpisz connection string, klucz JWT i ustawienia SMTP zmiennymi środowiskowymi lub plikiem `appsettings.Production.json`.

---

## 🧱 **Modele danych (przykładowe)**

```csharp
class Patient { string Pesel; string InsuranceNumber; ... }
class MedicalRecord { string DocumentScanUrl; ... }
class Visit { Status, AssignedDoctor, List<ProcedurePerformed>, List<ClinicalNote> }
class ProcedurePerformed { Description, ServiceCost, List<PrescribedMedication> }
class Medication { Name, UnitPrice }
class PrescribedMedication { Medication, Dosage, Quantity }
class ClinicalNote { Author, Content, Timestamp }
```

---

## 🛠️ **Wymagania techniczne**

| Obszar                  | Szczegóły                                                                 |
|-------------------------|---------------------------------------------------------------------------|
| **ASP.NET Core**        | Wersja 10 (.NET 10)                                                       |
| **EF Core**             | Code First + migracje - SQL Server                                        |
| **Identity**            | Logowanie, role, autoryzacja                                              |
| **Mapperly**            | Mapowanie DTO ↔️ encje np. Mapperly                                       |
| **DI**                  | Serwisy biznesowe (`IPatientService`, `IVisitService`, ...)               |
| **OpenAPI**             | Dokumentacja API                                                          |
| **Upload plików**       | Skany dokumentów (np. do `/wwwroot/uploads`)                              |
| **PDF**                 | Generowanie raportów jako PDF                                             |
| **Frontend**            | Razor Pages + Bootstrap (opcjonalnie SPA: React/Blazor/Angular)           |
| **Testy**               | testy jednostkowe (xUnit/NUnit)                                           |
| **CI/CD**               | GitHub Actions — build, test, Docker build                                |

---

## 🗂️ **Struktura projektu**

```
/ClinicManagerAPI
├── .github/
│   └── workflows/
│       └── dotnet-ci.yml    // pipeline CI/CD
├── ClinicManagerAPI.PerformanceTests/
│   ├── VisitsLoadTest.cs    // scenariusz NBomber
│   └── Program.cs
├── ClinicManagerAPI/
│   ├── Controllers/
│   ├── DTOs/
│   ├── Models/
│   ├── Services/
│   ├── Mappers/             // Mapperly mappery
│   ├── Views/
│   ├── wwwroot/
│   │   └── uploads/         // skany dokumentów medycznych
│   ├── Data/
│   └── Program.cs
├── Dockerfile
├── ClinicManagerAPI.slnx
└── README.md
```

---

## ✅ Co należy oddać?

- Repozytorium GitHub z historią commitów
- Działająca aplikacja ASP.NET Core
- Migracje + seed danych (lub dump bazy)
- `README.md` z opisem projektu, logowania, rolami

---

## 📌 Wskazówki

- Wszystkie dane domenowe mapuj za pomocą **Mapperly**
- Używaj **DataAnnotations** do walidacji
- Dbaj o **separację warstw**: logika w serwisach, nie w kontrolerach
- Pamiętaj, że dane medyczne to dane wrażliwe – w komentarzach do projektu warto wspomnieć o **RODO** (np. logowanie dostępu do kartoteki, brak twardego usuwania pacjentów – soft delete)
