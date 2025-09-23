# Stack Overflow Clone - Projekat iz Razvoj Cloud aplikacija u pametnim mrežama

Ovaj projekat je implementacija sistema za postavljanje pitanja i odgovora, inspirisan platformom Stack Overflow. Izrađen je kao projektni zadatak iz predmeta Razvoj Cloud aplikacija u pametnim mrežama.

Aplikacija je razvijena kao distribuirani sistem unutar **Azure Cloud Services (Classic)** okruženja, podeljena na više servisa koji komuniciraju asinhrono i obavljaju različite zadatke.

## Arhitektura Sistema

Sistem je zasnovan na trodelnoj arhitekturi, koristeći Azure Web i Worker Role:

1.  **`StackOverflowService` (Web Role)**
    * Predstavlja korisnički interfejs (UI) aplikacije, realizovan kao ASP.NET MVC projekat.
    * Zadužen je za sve direktne interakcije sa korisnikom: prikaz stranica, obradu formi (registracija, postavljanje pitanja, odgovaranje) i komunikaciju sa data slojem.
    * Sve spore operacije (slanje emailova) delegira `NotificationService`-u putem Azure Redova (Queues).

2.  **`HealthMonitoringService` (Worker Role)**
    * Pozadinski servis koji radi neprekidno i proverava dostupnost ostalih servisa (`StackOverflowService` i `NotificationService`).
    * Na svake 4 sekunde, šalje WCF (NetTcpBinding) zahteve ka endpointima drugih servisa.
    * Rezultate provere (OK ili NOT_OK) beleži u `HealthCheck` Azure Tabelu.
    * U slučaju neuspeha, šalje poruku u `alerts` red.
    * Pokrenut je u **2 instance** radi pouzdanosti.

3.  **`NotificationService` (Worker Role)**
    * Asinhroni pozadinski servis za obavljanje komunikacionih zadataka.
    * Sluša dva Azure Reda:
        * **`alerts`**: Prima poruke od `HealthMonitoringService`-a i šalje email upozorenja administratorima.
        * **`acceptedanswersqueue`**: Prima poruku sa ID-jem najboljeg odgovora, pronalazi sve učesnike u temi i šalje im notifikaciju da je pitanje zatvoreno.
    * Beleži sve poslate notifikacije u `NotificationLogs` Azure Tabelu.
    * Pokrenut je u **3 instance** radi pouzdanosti i brže obrade.

**Skladište podataka (Azure Storage):**
* **Azure Table Storage:** Koristi se za čuvanje svih strukturiranih podataka (Korisnici, Pitanja, Odgovori, Glasovi i Logovi).
* **Azure Blob Storage:** Koristi se za čuvanje nestrukturiranih podataka (slike profila i slike grešaka uz pitanja).
* **Azure Queue Storage:** Koristi se za asinhronu komunikaciju između servisa.

## Ključne Funkcionalnosti

-   ✅ **Upravljanje Korisnicima:** Kompletna registracija, prijava (sa hešovanjem lozinki), izmena profila (sa promenom slike).
-   ✅ **Sistem Pitanja:** Postavljanje, izmena i brisanje pitanja (sa autorizacijom).
-   ✅ **Sistem Odgovora:** Postavljanje odgovora na pitanja.
-   ✅ **Interaktivnost:** Glasanje za odgovore (sa sprečavanjem duplih glasova) i odabir najboljeg odgovora od strane autora pitanja.
-   ✅ **Notifikacije:** Automatsko slanje emailova administratorima u slučaju pada sistema i svim učesnicima kada je tema zatvorena.
-   ✅ **Korisnički Interfejs:** Sortiranje pitanja (po datumu, broju odgovora), pretraga po naslovu i moderan, responzivan dizajn.

## Tehnologije

* **Platforma:** .NET Framework 4.8, C#
* **Web:** ASP.NET MVC 5
* **Cloud:** Azure Cloud Services (Web Role, Worker Role)
* **Baza:** Azure Storage (Table, Blob, Queue)
* **Komunikacija:** WCF (NetTcpBinding), Azure Queue Storage
* **Email:** SMTP

## Pokretanje Projekta

1.  **Preduslovi:**
    * Visual Studio 2022 sa **Azure development** workload-om (uključujući **Azure Cloud Services support** komponentu).
    * Azure Storage Explorer (preporučeno za pregled baze).

2.  **Konfiguracija:**
    * Klonirati repozitorijum.
    * U `ServiceConfiguration.Local.cscfg` fajlovima za **sva tri servisa**, podesiti `DataConnectionString` na `UseDevelopmentStorage=true`.
    * U `ServiceConfiguration.Local.cscfg` za **NotificationService**, uneti ispravne kredencijale za `EmailUsername` i `EmailPassword`.

3.  **Pokretanje:**
    * Otvoriti `.sln` fajl u Visual Studiju.
    * Uraditi **Restore NuGet Packages**.
    * Uraditi **Rebuild Solution**.
    * Postaviti `StackOverflowProject` kao StartUp Project.
    * Pokrenuti sa F5. (Možda će biti potrebno pokrenuti Visual Studio kao administrator ako se koristi "Full Emulator").

4.  **Popunjavanje Baze:**
    * Aplikacija sadrži `DataSeeder` koji će pri prvom pokretanju automatski napuniti bazu testnim podacima. Da bi se ovo ponovo pokrenulo, potrebno je obrisati tabele (`Users`, `Questions`, `Answers`, `Votes`) u Azure Storage Exploreru.
