using ClinicManagerAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagerAPI.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await SeedUserAsync(userManager, "admin@clinic.pl", "Admin123!", "Adam", "Administrator", null, "Admin");
            var lekarz = await SeedUserAsync(userManager, "lekarz@clinic.pl", "Lekarz123!", "Andrzej", "Nowak", "Internista", "Lekarz");
            await SeedUserAsync(userManager, "rejestratorka@clinic.pl", "Rejestratorka123!", "Karolina", "Kowalska", null, "Rejestratorka");

            if (await context.Patients.AnyAsync())
            {
                return;
            }

            var medications = SeedMedications(context);
            var patients = SeedPatients(context);

            await context.SaveChangesAsync();

            var visits = SeedVisits(context, patients, lekarz.Id);
            await context.SaveChangesAsync();

            SeedProcedures(context, visits, medications);
            SeedClinicalNotes(context, visits, lekarz);

            await context.SaveChangesAsync();
        }

        private static async Task<ApplicationUser> SeedUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string firstName,
            string lastName,
            string? specialization,
            string role)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser is not null)
            {
                if (!await userManager.IsInRoleAsync(existingUser, role))
                {
                    await userManager.AddToRoleAsync(existingUser, role);
                }

                return existingUser;
            }

            var user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                FirstName = firstName,
                LastName = lastName,
                Specialization = specialization,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Nie udało się utworzyć użytkownika {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await userManager.AddToRoleAsync(user, role);
            return user;
        }

        private static List<Medication> SeedMedications(ApplicationDbContext context)
        {
            var medications = new List<Medication>
            {
                new() { Name = "Ibuprofen", UnitPrice = 12.50m },
                new() { Name = "Amoksycylina", UnitPrice = 25.00m },
                new() { Name = "Metformina", UnitPrice = 18.75m },
                new() { Name = "Paracetamol", UnitPrice = 8.00m },
                new() { Name = "Omeprazol", UnitPrice = 22.30m },
                new() { Name = "Atorwastatyna", UnitPrice = 35.50m },
                new() { Name = "Losartan", UnitPrice = 28.90m },
                new() { Name = "Amlodypina", UnitPrice = 19.60m },
                new() { Name = "Sertralina", UnitPrice = 42.00m },
                new() { Name = "Salbutamol", UnitPrice = 31.25m }
            };

            context.Medications.AddRange(medications);
            return medications;
        }

        private static List<Patient> SeedPatients(ApplicationDbContext context)
        {
            var patients = new List<Patient>
            {
                new()
                {
                    FirstName = "Jan",
                    LastName = "Kowalski",
                    Pesel = "85010112345",
                    InsuranceNumber = "NFZ123456789",
                    CreatedAt = DateTime.UtcNow.AddMonths(-6)
                },
                new()
                {
                    FirstName = "Anna",
                    LastName = "Nowak",
                    Pesel = "92031554321",
                    InsuranceNumber = "NFZ987654321",
                    CreatedAt = DateTime.UtcNow.AddMonths(-4)
                },
                new()
                {
                    FirstName = "Piotr",
                    LastName = "Wiśniewski",
                    Pesel = "78042298765",
                    InsuranceNumber = "NFZ456789123",
                    CreatedAt = DateTime.UtcNow.AddMonths(-3)
                },
                new()
                {
                    FirstName = "Maria",
                    LastName = "Wójcik",
                    Pesel = "65120311223",
                    InsuranceNumber = "NFZ321654987",
                    CreatedAt = DateTime.UtcNow.AddMonths(-2)
                },
                new()
                {
                    FirstName = "Tomasz",
                    LastName = "Dąbrowski",
                    Pesel = "03051233445",
                    InsuranceNumber = "NFZ654321098",
                    CreatedAt = DateTime.UtcNow.AddMonths(-1)
                }
            };

            context.Patients.AddRange(patients);
            return patients;
        }

        private static List<Visit> SeedVisits(ApplicationDbContext context, List<Patient> patients, string doctorId)
        {
            var today = DateTime.UtcNow.Date;
            var visits = new List<Visit>
            {
                new()
                {
                    Patient = patients[0],
                    Date = today.AddDays(1),
                    Status = VisitStatus.Zaplanowana,
                    AssignedDoctorId = doctorId
                },
                new()
                {
                    Patient = patients[1],
                    Date = today,
                    Status = VisitStatus.WTrakcie,
                    AssignedDoctorId = doctorId
                },
                new()
                {
                    Patient = patients[2],
                    Date = today.AddDays(-7),
                    Status = VisitStatus.Zakonczona,
                    AssignedDoctorId = doctorId
                },
                new()
                {
                    Patient = patients[3],
                    Date = today.AddDays(-14),
                    Status = VisitStatus.Zakonczona,
                    AssignedDoctorId = doctorId
                },
                new()
                {
                    Patient = patients[4],
                    Date = today.AddDays(5),
                    Status = VisitStatus.Anulowana,
                    AssignedDoctorId = doctorId
                },
                new()
                {
                    Patient = patients[0],
                    Date = today.AddDays(-30),
                    Status = VisitStatus.Zakonczona,
                    AssignedDoctorId = doctorId
                },
                new()
                {
                    Patient = patients[1],
                    Date = today.AddDays(7),
                    Status = VisitStatus.Zaplanowana,
                    AssignedDoctorId = doctorId
                },
                new()
                {
                    Patient = patients[2],
                    Date = today.AddDays(2),
                    Status = VisitStatus.Zaplanowana,
                    AssignedDoctorId = doctorId
                }
            };

            context.Visits.AddRange(visits);
            return visits;
        }

        private static void SeedProcedures(
            ApplicationDbContext context,
            List<Visit> visits,
            List<Medication> medications)
        {
            var procedures = new List<ProcedurePerformed>
            {
                new()
                {
                    Visit = visits[1],
                    Description = "Badanie ogólne i pomiar ciśnienia",
                    ServiceCost = 120.00m,
                    PrescribedMedications =
                    [
                        new()
                        {
                            Medication = medications[0],
                            Dosage = "400 mg co 8 godzin",
                            Quantity = 20
                        }
                    ]
                },
                new()
                {
                    Visit = visits[2],
                    Description = "Konsultacja internistyczna",
                    ServiceCost = 150.00m,
                    PrescribedMedications =
                    [
                        new()
                        {
                            Medication = medications[1],
                            Dosage = "500 mg co 12 godzin",
                            Quantity = 14
                        },
                        new()
                        {
                            Medication = medications[3],
                            Dosage = "500 mg w razie bólu",
                            Quantity = 10
                        }
                    ]
                },
                new()
                {
                    Visit = visits[3],
                    Description = "EKG spoczynkowe",
                    ServiceCost = 80.00m
                },
                new()
                {
                    Visit = visits[5],
                    Description = "Kontrola cukrzycy i lipidogram",
                    ServiceCost = 200.00m,
                    PrescribedMedications =
                    [
                        new()
                        {
                            Medication = medications[2],
                            Dosage = "500 mg 2 razy dziennie",
                            Quantity = 60
                        },
                        new()
                        {
                            Medication = medications[5],
                            Dosage = "20 mg wieczorem",
                            Quantity = 30
                        }
                    ]
                }
            };

            context.ProceduresPerformed.AddRange(procedures);
        }

        private static void SeedClinicalNotes(
            ApplicationDbContext context,
            List<Visit> visits,
            ApplicationUser lekarz)
        {
            var author = $"{lekarz.FirstName} {lekarz.LastName}";

            var notes = new List<ClinicalNote>
            {
                new()
                {
                    Visit = visits[1],
                    Author = author,
                    Content = "Wywiad: Pacjent zgłasza ból głowy od 2 dni. Rozpoznanie: Nadciśnienie tętnicze. Zalecenia: Kontrola ciśnienia, dieta niskosodowa.",
                    Timestamp = DateTime.UtcNow.AddHours(-2)
                },
                new()
                {
                    Visit = visits[1],
                    Author = author,
                    Content = "Wywiad: Brak alergii lekowych. Rozpoznanie: Stan ogólny dobry. Zalecenia: Kontynuacja leczenia przeciwbólowego.",
                    Timestamp = DateTime.UtcNow.AddHours(-1)
                },
                new()
                {
                    Visit = visits[2],
                    Author = author,
                    Content = "Wywiad: Kaszel produktywny, gorączka 38.5°C. Rozpoznanie: Infekcja górnych dróg oddechowych. Zalecenia: Antybiotykoterapia, nawodnienie, odpoczynek.",
                    Timestamp = visits[2].Date.AddHours(1)
                },
                new()
                {
                    Visit = visits[3],
                    Author = author,
                    Content = "Wywiad: Duszności przy wysiłku. Rozpoznanie: Podejrzenie nadciśnienia tętniczego. Zalecenia: Holter ciśnieniowy, kontrola za 2 tygodnie.",
                    Timestamp = visits[3].Date.AddHours(2)
                },
                new()
                {
                    Visit = visits[5],
                    Author = author,
                    Content = "Wywiad: Cukrzyca typu 2 od 5 lat. Rozpoznanie: Cukrzyca typu 2 ze stabilną kontrolą. Zalecenia: Kontynuacja metforminy, dieta, kontrola HbA1c za 3 miesiące.",
                    Timestamp = visits[5].Date.AddHours(1)
                },
                new()
                {
                    Visit = visits[5],
                    Author = author,
                    Content = "Wywiad: Podwyższone cholesterol LDL. Rozpoznanie: Hiperlipidemia. Zalecenia: Atorwastatyna, dieta niskotłuszczowa, kontrola lipidogramu.",
                    Timestamp = visits[5].Date.AddHours(2)
                }
            };

            context.ClinicalNotes.AddRange(notes);
        }
    }
}
