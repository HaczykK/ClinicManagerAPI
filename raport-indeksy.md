# Raport: Indeksy nieklastrowane — optymalizacja zapytań

**Projekt:** ClinicManagerAPI  
**Baza danych:** ClinicManagerDB (`(localdb)\mssqllocaldb`)  
**Migracja:** `20260609165030_AddNonClusteredIndexes`

---

## 1. Cel

Dodanie indeksów nieklastrowanych (non-clustered) do kolumn często używanych w klauzulach `WHERE` i `JOIN`, aby przyspieszyć typowe zapytania SELECT w systemie zarządzania przychodnią.

---

## 2. Zidentyfikowane kolumny i uzasadnienie

| Tabela | Kolumna | Indeks | Uzasadnienie (kod aplikacji) |
|--------|---------|--------|------------------------------|
| Patients | Pesel | UNIQUE | `PatientService.EnsurePeselIsUniqueAsync` — wyszukiwanie po PESEL |
| Patients | LastName | — | `PatientService.SearchAsync` — wyszukiwanie po nazwisku |
| Visits | Date | — | `VisitService.GetAllAsync`, raporty dzienne — filtrowanie po dacie |
| Visits | AssignedDoctorId | — (już istniał FK) | `VisitService.GetByDoctorIdAsync` — wizyty lekarza |
| Visits | PatientId | — (już istniał FK) | `VisitService.GetByPatientIdAsync` — lista wizyt pacjenta |
| Visits | Status | — | `VisitService.GetAllAsync` — filtrowanie po statusie |

---

## 3. Zapytanie testowe 1 — wyszukiwanie pacjenta po PESEL

```sql
SET STATISTICS IO ON;

SELECT Id, FirstName, LastName, Pesel
FROM Patients
WHERE Pesel = '85010112345' AND IsDeleted = 0;
```

### Plan PRZED migracją

```
|--Clustered Index Scan (PK_Patients)
   WHERE: IsDeleted = 0 AND Pesel = '85010112345'
```

- **Operacja:** Clustered Index Scan (pełne przeszukanie tabeli)
- **Logical reads:** 3

### Plan PO migracji

```
|--Nested Loops (Inner Join)
   |--Index Seek (IX_Patients_Pesel) — SEEK po Pesel
   |--Clustered Index Seek (PK_Patients) — Key Lookup po Id + filtr IsDeleted
```

- **Operacja:** Index Seek + Key Lookup
- **Logical reads:** 4

### Porównanie

| Metryka | Przed | Po |
|---------|-------|-----|
| Główna operacja | Clustered Index Scan | **Index Seek** na `IX_Patients_Pesel` |
| Logical reads | 3 | 4 |

**Wniosek:** Indeks unikalny na `Pesel` pozwala optymalizatorowi przejść ze skanowania całej tabeli na precyzyjne wyszukiwanie (Seek). Przy większej liczbie pacjentów różnica w wydajności będzie znacznie większa.

---

## 4. Zapytanie testowe 2 — wizyty na dany dzień

```sql
SET STATISTICS IO ON;

DECLARE @dayStart datetime2 = CAST(GETDATE() AS date);
DECLARE @dayEnd   datetime2 = DATEADD(day, 1, @dayStart);

SELECT Id, Date, Status, PatientId, AssignedDoctorId
FROM Visits
WHERE Date >= @dayStart AND Date < @dayEnd;
```

### Plan PRZED migracją

```
|--Clustered Index Scan (PK_Visits)
   WHERE: Date >= @dayStart AND Date < @dayEnd
```

- **Operacja:** Clustered Index Scan
- **Logical reads:** 3

### Plan PO migracji

```
|--Clustered Index Scan (PK_Visits)
   WHERE: Date >= @dayStart AND Date < @dayEnd
```

- **Operacja:** Clustered Index Scan (bez zmiany przy małej tabeli — 8 wierszy)
- **Logical reads:** 3
- **Indeks utworzony:** `IX_Visits_Date` (potwierdzony przez `sp_helpindex`)

### Porównanie

| Metryka | Przed | Po |
|---------|-------|-----|
| Główna operacja | Clustered Index Scan | Clustered Index Scan |
| Logical reads | 3 | 3 |
| Indeks `IX_Visits_Date` | brak | **utworzony** |

**Wniosek:** Przy bardzo małej tabeli (8 rekordów) optymalizator SQL Server świadomie wybiera Scan jako tańszy plan kosztowo. Indeks `IX_Visits_Date` jest dostępny i zostanie użyty (Index Seek) przy większej liczbie wizyt — typowe zachowanie SQL Servera.

---

## 5. Utworzone indeksy (po migracji)

**Patients:**
- `IX_Patients_Pesel` — nonclustered, **unique**
- `IX_Patients_LastName` — nonclustered

**Visits:**
- `IX_Visits_Date` — nonclustered
- `IX_Visits_Status` — nonclustered
- `IX_Visits_AssignedDoctorId` — istniejący (FK)
- `IX_Visits_PatientId` — istniejący (FK)

---

## 6. Podsumowanie

1. **Największa poprawa:** zapytanie po PESEL — wyraźna zmiana z **Scan** na **Index Seek** dzięki unikalnemu indeksowi `IX_Patients_Pesel`.
2. **Indeks na Date:** utworzony poprawnie; przy obecnej wielkości danych optymalizator preferuje Scan — oczekiwane zachowanie.
3. **Indeks na LastName i Status:** gotowe do użycia przy zapytaniach filtrujących po tych kolumnach.
4. **Konfiguracja EF Core:** indeksy zdefiniowane w `ApplicationDbContext.OnModelCreating`, migracja `AddNonClusteredIndexes`.

---

## Załączniki (plany zapytań — output sqlcmd)

- `docs/analysis/before-query1-pesel.txt`
- `docs/analysis/after-query1-pesel.txt`
- `docs/analysis/before-query2-visits-date.txt`
- `docs/analysis/after-query2-visits-date.txt`
- `docs/analysis/comparison-summary.txt`
