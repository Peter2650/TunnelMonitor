using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Newtonsoft.Json;


namespace TunnelMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<PersonEntry> personsInTunnel;
        private DispatcherTimer checkTimer = new DispatcherTimer();
        private FileSystemWatcher watcher = new FileSystemWatcher();
        private string sharedFolderPath = "C:\\TunnelMonitorData";
        private LogManager log = new LogManager();

        public MainWindow()
        {
            InitializeComponent();
            personsInTunnel = new ObservableCollection<PersonEntry>();
            dgPersonsInTunnel.ItemsSource = personsInTunnel;

            LoadDataDirPath();
            // sharedFolderPath = @"C:\Users\peter\OneDrive\Dokumenter\projects\TunnelMonitorData";
            LoadExistingPersons();
            if (string.IsNullOrEmpty(sharedFolderPath))
            {
                return;
            }
            log.LogPath = sharedFolderPath;

            // Initialiser og start DispatcherTimer
            checkTimer.Interval = TimeSpan.FromSeconds(30); // Tjek hver 30. sekund
            checkTimer.Tick += CheckTimer_Tick;
            checkTimer.Start();

            // Initialiser og start FileSystemWatcher
            watcher.Path = sharedFolderPath;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.txt";
            watcher.Changed += OnChanged;
            watcher.EnableRaisingEvents = true;
            // Indlæs eksisterende personfiler ved opstart
        }

        private void CheckTimer_Tick(object? sender, EventArgs e)
        {
            // Gennemgå hver person i listen og tjek deres forventede returtid
            foreach (var person in personsInTunnel)
            {
                if (DateTime.Now > person.ExpectedReturnTime && !person.IsOverdue)
                {
                    // Marker personen som overdue
                    person.IsOverdue = true;
                }
                else if (DateTime.Now <= person.ExpectedReturnTime && person.IsOverdue)
                {
                    // Fjern overdue markeringen, hvis personen ikke længere er overdue
                    person.IsOverdue = false;
                }
            }

            // Opdater UI trådsikkert
            Dispatcher.Invoke(() =>
            {
                dgPersonsInTunnel.Items.Refresh();
            });
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Tjek om tekstboksen for navn har nogen tekst
            if (!string.IsNullOrWhiteSpace(txtName.Text))
            {
                // Sæt den nuværende tid i 'Indgangstid', hvis den ikke allerede er sat
                if (string.IsNullOrWhiteSpace(txtEntryTime.Text))
                {
                    txtEntryTime.Text = DateTime.Now.ToString("dd-MM-yyyy HH:mm");
                }

                // Sæt tiden for 'Forventet retur' til en time senere, hvis den ikke allerede er sat
                if (string.IsNullOrWhiteSpace(txtExpectedReturnTime.Text))
                {
                    txtExpectedReturnTime.Text = DateTime
                        .Now
                        .AddHours(1)
                        .ToString("dd-MM-yyyy HH:mm");
                }
                txtNumber.Text = "1";
            }
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Application
                .Current
                .Dispatcher
                .Invoke(() =>
                {
                    try
                    {
                        string fileName = System.IO.Path.GetFileName(e.FullPath);

                        // Find personen i samlingen baseret på filnavnet
                        var person = personsInTunnel.FirstOrDefault(p => p.FileName == fileName);

                        if (e.ChangeType == WatcherChangeTypes.Deleted)
                        {
                            if (person != null)
                            {
                                personsInTunnel.Remove(person);
                            }
                        }
                        else // Hvis filen er blevet oprettet eller ændret
                        {
                            string[] lines = File.ReadAllLines(e.FullPath);
                            var personData = new Dictionary<string, string>();

                            foreach (var line in lines)
                            {
                                var keyValue = line.Split(new[] { ':' }, 2);
                                if (keyValue.Length == 2)
                                {
                                    personData[keyValue[0].Trim()] = keyValue[1].Trim();
                                }
                            }

                            string name = personData["Name"];
                            string phone = personData["Phone"];
                            string company = personData["Company"];
                            int numberOfPersons = int.Parse(personData["NumberOfPersons"]);
                            bool isTunnel1Checked = personData["Tunnel1"] == "True";
                            bool isTunnel2Checked = personData["Tunnel2"] == "True";
                            DateTime entryTime = DateTime.Parse(personData["EntryTime"]);
                            DateTime expectedReturn = DateTime.Parse(
                                personData["ExpectedReturnTime"]
                            );

                            if (person == null) // Hvis personen ikke findes, tilføj dem
                            {
                                person = new PersonEntry(
                                    name,
                                    phone,
                                    company,
                                    numberOfPersons,
                                    isTunnel1Checked,
                                    isTunnel2Checked,
                                    entryTime,
                                    expectedReturn,
                                    fileName
                                );
                                personsInTunnel.Add(person);
                            }
                            else // Hvis personen allerede findes, opdater deres oplysninger
                            {
                                person.Company = company;
                                person.EntryTime = entryTime;
                                person.NumberOfPersons = numberOfPersons;
                                person.IsTunnel1Checked = isTunnel1Checked;
                                person.IsTunnel2Checked = isTunnel2Checked;
                                person.ExpectedReturnTime = expectedReturn;
                                person.IsOverdue = DateTime.Now > expectedReturn;
                            }
                        }
                    }
                    catch
                    {
                        // Håndter eventuelle IO fejl
                    }
                });
        }

        private void btnAddPerson_Click(object sender, RoutedEventArgs e)
        {
            // Valider input (du kan tilføje mere avanceret validering efter behov)
            // Tjekker for tomme felter
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("The name field must not be empty.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("The phone field must not be empty.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtCompany.Text))
            {
                MessageBox.Show("The company field must not be empty.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNumber.Text))
            {
                MessageBox.Show("The number field must not be empty.");
                return;
            }
            if (!int.TryParse(txtNumber.Text, out int numberOfPersons))
            {
                // Handle the error if the text is not a valid integer
                MessageBox.Show("Please enter a valid number of persons.");
                return;
            }
            if (chkTunnel1.IsChecked == false && chkTunnel2.IsChecked == false)
            {
                MessageBox.Show("Please select a tunnel.");
                return;
            }

            // Validerer datoer med et specifikt format
            string dateFormat = "dd-MM-yyyy HH:mm";
            if (
                !DateTime.TryParseExact(
                    txtEntryTime.Text,
                    dateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime entryTime
                )
            )
            {
                MessageBox.Show("Entry time has an invalid format. Use the format: " + dateFormat);
                return;
            }
            if (
                !DateTime.TryParseExact(
                    txtExpectedReturnTime.Text,
                    dateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime expectedReturnTime
                )
            )
            {
                MessageBox.Show(
                    "The expected return date has an invalid format. Use the format: " + dateFormat
                );
                return;
            }

            // Hvis koden når her, er alle felter validerede uden fejl.


            // Opret en ny PersonEntry
            var newPerson = new PersonEntry(
                txtName.Text,
                txtPhone.Text,
                txtCompany.Text,
                numberOfPersons,
                chkTunnel1.IsChecked ?? false,
                chkTunnel2.IsChecked ?? false,
                entryTime,
                expectedReturnTime
            );

            // Tilføj den nye person til ObservableCollection
            personsInTunnel.Add(newPerson);
            log.LogEntry(newPerson);

            // Gem personens data til den delte fil
            SavePersonToFile(newPerson);

            // Ryd inputfelterne
            txtName.Clear();
            txtPhone.Clear();
            txtCompany.Clear();
            txtNumber.Clear();
            chkTunnel1.IsChecked = false;
            chkTunnel2.IsChecked = false;
            txtEntryTime.Clear();
            txtExpectedReturnTime.Clear();
        }

        private void RemovePerson_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var person = button?.DataContext as PersonEntry;
            if (person != null)
            {
                personsInTunnel.Remove(person);
                File.Delete(System.IO.Path.Combine(sharedFolderPath, person.FileName));
                log.LogExit(person);
            }
        }

        private void LoadExistingPersons()
        {
            // Tjek om mappen eksisterer
            if (Directory.Exists(sharedFolderPath))
            {
                // Hent alle .txt filer i mappen
                var files = Directory.GetFiles(sharedFolderPath, "*.txt");
                foreach (var file in files)
                {
                    try
                    {
                        // Læs filen og skab et PersonEntry objekt
                        string[] lines = File.ReadAllLines(file);
                        var personData = new Dictionary<string, string>();
                        foreach (var line in lines)
                        {
                            var keyValue = line.Split(new[] { ':' }, 2);
                            if (keyValue.Length == 2)
                            {
                                personData[keyValue[0].Trim()] = keyValue[1].Trim();
                            }
                        }

                        // Antag at alle nødvendige nøgler findes for enkelhedens skyld
                        var name = personData["Name"];
                        var phone = personData["Phone"];
                        var company = personData["Company"];
                        var numberOfPersons = int.Parse(personData["NumberOfPersons"]);
                        var isTunnel1Checked = personData["Tunnel1"] == "True";
                        var isTunnel2Checked = personData["Tunnel2"] == "True";
                        var entryTime = DateTime.ParseExact(
                            personData["EntryTime"],
                            "dd-MM-yyyy HH:mm",
                            CultureInfo.InvariantCulture
                        );
                        var expectedReturn = DateTime.ParseExact(
                            personData["ExpectedReturnTime"],
                            "dd-MM-yyyy HH:mm",
                            CultureInfo.InvariantCulture
                        );

                        // Opret et nyt PersonEntry objekt og tilføj det til kollektionen
                        var personEntry = new PersonEntry(
                            name,
                            phone,
                            company,
                            numberOfPersons,
                            isTunnel1Checked,
                            isTunnel2Checked,
                            entryTime,
                            expectedReturn,
                            System.IO.Path.GetFileName(file)
                        );
                        personsInTunnel.Add(personEntry);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Error loading person data from: " + file);
                    }
                }
            }
            Dispatcher.Invoke(() =>
            {
                dgPersonsInTunnel.Items.Refresh();
            });
        }
        // }

        private void SavePersonToFile(PersonEntry person)
        {
            // Sti til den delte mappe

            // Generer filnavnet baseret på personens data
            string fileName = person.FileName;

            // Skriv personens data til filen i det angivne format
            var sb = new StringBuilder();
            sb.AppendLine($"Name: {person.Name}");
            sb.AppendLine($"Phone: {person.Phone}");
            sb.AppendLine($"Company: {person.Company}");
            sb.AppendLine($"NumberOfPersons: {person.NumberOfPersons}");
            sb.AppendLine($"Tunnel1: {person.IsTunnel1Checked.ToString()}");
            sb.AppendLine($"Tunnel2: {person.IsTunnel2Checked.ToString()}");
            sb.AppendLine($"EntryTime: {person.EntryTime:dd-MM-yyyy HH:mm}");
            sb.AppendLine($"ExpectedReturnTime: {person.ExpectedReturnTime:dd-MM-yyyy HH:mm}");

            // Gem dataene til filen
            File.WriteAllText(System.IO.Path.Combine(sharedFolderPath, fileName), sb.ToString());
        }

        private void LoadDataDirPath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string settingsDirectoryPath = System.IO.Path.Combine(appDataPath, "TunnelMonitor");
            string settingsFilePath = System.IO.Path.Combine(settingsDirectoryPath, "settings.json");

            // Kontroller, om settings.json filen eksisterer
            if (File.Exists(settingsFilePath))
            {
                // Parse JSON fra filen
                string jsonContent = File.ReadAllText(settingsFilePath);
                var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);

                // Antager at der er en nøgle i JSON kaldet 'SharedFolderPath'
                if (settings != null && settings.ContainsKey("DataDir"))
                {
                    sharedFolderPath = settings["DataDir"].Replace("%HOMEDIR%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                    Directory.CreateDirectory(sharedFolderPath);
                }
                else
                {
                    // Hvis nøglen ikke findes, håndter fejlen
                    MessageBox.Show("Nøglen 'DataDir' blev ikke fundet i settings.json.");
                }
            }
            else
            {
                // Filen findes ikke, så vis en fejlbesked
                MessageBox.Show(
                    @"Filen ""TunnelMonitor/settings.json"" findes ikke.
        Gå til " + settingsDirectoryPath + @"
        Opret ""settings.json"" og skriv JSON-indholdet med nødvendige indstillinger.
        Eksempel på indhold i 'settings.json':
        {
            ""SharedFolderPath"": ""E:\\TunnelMonitorData""
        }"
                );
            }
        }
    }
}
