using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lab2XmlAnalysis.Models;
using Lab2XmlAnalysis.Services;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using System.Text.RegularExpressions;

namespace Lab2XmlAnalysis.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public ObservableCollection<string> AnalysisTypes { get; }
        [ObservableProperty] private string selectedAnalysisType;

        [ObservableProperty] private ObservableCollection<string> faculties;
        [ObservableProperty] private string selectedFaculty;

        [ObservableProperty] private ObservableCollection<string> departments;
        [ObservableProperty] private string selectedDepartment;

        [ObservableProperty] private string authorQuery;
        [ObservableProperty] private string titleQuery;

        [ObservableProperty] private ObservableCollection<Material> searchResultsList;
        [ObservableProperty] private Material? selectedMaterial;
        [ObservableProperty] private string htmlContent;

        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private bool isMaterialSelected;
        [ObservableProperty] private bool isPlaceholderVisible;
        [ObservableProperty] private string foundCountText;
        [ObservableProperty] private bool isNoResultsFound;

        [ObservableProperty] private ObservableCollection<string> archiveList;
        [ObservableProperty] private string selectedArchive;
        [ObservableProperty] private bool isDeleteEnabled;

        public ObservableCollection<string> SortOptions { get; }
        [ObservableProperty] private string selectedSortOption;

        private string loadedXmlContent = "";
        private string xslContent = "";
        private readonly Dictionary<string, IAnalysisStrategy> analysisStrategies;
        private const string DefaultArchiveDisplayName = "Стандартний архів (вбудований)";

        private const string BaseHtmlStyle = "background-color: white; font-family: 'Segoe UI', system-ui, sans-serif; padding: 15px; color: black;";
        private const string DefaultHtmlContent = $@"<html style=""{BaseHtmlStyle}""><body style='margin:0; padding:0;'><h2 style='font-weight: 400; margin-bottom: 10px;'>HTML Результат Трансформації</h2><p style='font-size: 14px; color: #666;'>*Тут з'явиться таблиця...</p></body></html>";

        public MainViewModel()
        {
            analysisStrategies = new Dictionary<string, IAnalysisStrategy>
            {
                { "SAX", new SaxAnalysisStrategy() },
                { "DOM", new DomAnalysisStrategy() },
                { "LINQ to XML", new LinqToXmlAnalysisStrategy() }
            };
            AnalysisTypes = new ObservableCollection<string> { "SAX", "DOM", "LINQ to XML" };
            SelectedAnalysisType = AnalysisTypes.First();

            SortOptions = new ObservableCollection<string> { "За назвою (А-Я)", "За автором (А-Я)", "За датою (Нові)" };
            SelectedSortOption = SortOptions.First();

            Faculties = new ObservableCollection<string>();
            Departments = new ObservableCollection<string>();
            SearchResultsList = new ObservableCollection<Material>();
            ArchiveList = new ObservableCollection<string>();

            SelectedFaculty = string.Empty; SelectedDepartment = string.Empty;
            AuthorQuery = string.Empty; TitleQuery = string.Empty;
            SelectedMaterial = null; HtmlContent = DefaultHtmlContent; IsBusy = false;
            IsMaterialSelected = false; IsPlaceholderVisible = true; IsNoResultsFound = false; FoundCountText = "";

            _ = LoadFilesAsync();
        }

        private async Task LoadFilesAsync()
        {
            try
            {
                using var xslStream = await FileSystem.OpenAppPackageFileAsync("ArchiveStyle.xsl");
                using var xslReader = new StreamReader(xslStream);
                xslContent = await xslReader.ReadToEndAsync();

                string[] filesToDeploy = { "Archive.xml", "BigArchive.xml" };

                foreach (var fileName in filesToDeploy)
                {
                    if (await FileSystem.AppPackageFileExistsAsync(fileName))
                    {
                        string targetPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                        if (!File.Exists(targetPath))
                        {
                            using var sourceStream = await FileSystem.OpenAppPackageFileAsync(fileName);
                            using var destStream = File.Create(targetPath);
                            await sourceStream.CopyToAsync(destStream);
                        }
                    }
                }
                InitializeArchivesList();
            }
            catch (Exception ex) { await Application.Current.MainPage.DisplayAlert("Помилка ініціалізації", ex.Message, "OK"); }
        }

        private void InitializeArchivesList()
        {
            ArchiveList.Clear();
            ArchiveList.Add(DefaultArchiveDisplayName);

            string appDataPath = FileSystem.AppDataDirectory;
            if (Directory.Exists(appDataPath))
            {
                var files = Directory.GetFiles(appDataPath, "*.xml");
                foreach (var file in files)
                {
                    var name = Path.GetFileName(file);
                    if (name != "Archive.xml") ArchiveList.Add(name);
                }
            }
            SelectedArchive = DefaultArchiveDisplayName;
        }

        async partial void OnSelectedArchiveChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            IsDeleteEnabled = value != DefaultArchiveDisplayName;
            await LoadSelectedXmlContent(value);
            await Clear();
        }

        async partial void OnSelectedSortOptionChanged(string value)
        {
            if (SearchResultsList != null && SearchResultsList.Any())
            {
                await ApplySortAndDisplay(SearchResultsList.ToList());
            }
        }

        private async Task LoadSelectedXmlContent(string archiveName)
        {
            IsBusy = true;
            try
            {
                string fileNameToLoad = archiveName == DefaultArchiveDisplayName ? "Archive.xml" : archiveName;
                string path = Path.Combine(FileSystem.AppDataDirectory, fileNameToLoad);

                if (File.Exists(path)) loadedXmlContent = await File.ReadAllTextAsync(path);
                else
                {
                    if (archiveName == DefaultArchiveDisplayName)
                    {
                        using var stream = await FileSystem.OpenAppPackageFileAsync("Archive.xml");
                        using var reader = new StreamReader(stream);
                        loadedXmlContent = await reader.ReadToEndAsync();
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Помилка", "Файл не знайдено.", "OK");
                        return;
                    }
                }
                ParseAttributesFromXml(loadedXmlContent);
            }
            catch (Exception ex) { await Application.Current.MainPage.DisplayAlert("Помилка XML", ex.Message, "OK"); }
            finally { IsBusy = false; }
        }

        private void ParseAttributesFromXml(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return;
            try
            {
                XDocument doc = XDocument.Parse(xml);
                var facultyList = doc.Descendants("Author").Select(a => a.Element("Faculty")?.Value).Where(f => !string.IsNullOrEmpty(f)).Distinct().OrderBy(f => f).ToList();
                var departmentList = doc.Descendants("Author").Select(a => a.Element("Department")?.Value).Where(d => !string.IsNullOrEmpty(d)).Distinct().OrderBy(d => d).ToList();
                MainThread.BeginInvokeOnMainThread(() => {
                    Faculties.Clear(); Departments.Clear();
                    Faculties.Add("Всі факультети"); Departments.Add("Всі кафедри");
                    foreach (var f in facultyList) Faculties.Add(f);
                    foreach (var d in departmentList) Departments.Add(d);
                    SelectedFaculty = Faculties.First(); SelectedDepartment = Departments.First();
                });
            }
            catch (Exception)
            {
                // Ігноруємо помилки парсингу при завантаженні атрибутів
            }
        }

        private string GenerateXmlFromResults(IEnumerable<Material> materials)
        {
            var xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("Archive", materials.Select(m => new XElement("Material", new XAttribute("Title", m.Title), new XAttribute("Type", m.Type), new XAttribute("CreationDate", m.CreationDate), new XAttribute("Volume", m.Volume), new XElement("Author", new XElement("FullName", m.Author.FullName), new XElement("Faculty", m.Author.Faculty), new XElement("Department", m.Author.Department))))));
            return xDoc.ToString();
        }

        // === ВИПРАВЛЕНИЙ МЕТОД ДЛЯ КНОПКИ "Відкрити файл XML" ===
        [RelayCommand]
        private async Task OpenFile()
        {
            try
            {
                // Створюємо кастомний тип файлу для XML
                var xmlFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "public.xml" } },
                        { DevicePlatform.Android, new[] { "application/xml", "text/xml" } },
                        { DevicePlatform.WinUI, new[] { ".xml" } },
                        { DevicePlatform.macOS, new[] { "xml" } },
                    });

                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Виберіть XML файл для аналізу",
                    FileTypes = xmlFileType
                });

                if (result != null)
                {
                    IsBusy = true;
                    using var stream = await result.OpenReadAsync();
                    using var reader = new StreamReader(stream);
                    loadedXmlContent = await reader.ReadToEndAsync();

                    ParseAttributesFromXml(loadedXmlContent);
                    await Clear();

                    await Application.Current.MainPage.DisplayAlert("Файл завантажено", $"Файл '{result.FileName}' успішно прочитано.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", $"Не вдалося відкрити файл: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        // ===================================================

        [RelayCommand]
        private async Task OpenArchiveFolder()
        {
            string path = FileSystem.AppDataDirectory; await Launcher.OpenAsync(new Uri($"file:///{path}"));
        }

        [RelayCommand]
        private async Task SaveFilteredArchive()
        {
            if (SearchResultsList == null || !SearchResultsList.Any()) { await Application.Current.MainPage.DisplayAlert("Увага", "Список пустий.", "OK"); return; }
            string fileName = "MyArchive"; string finalPath = "";
            while (true)
            {
                string inputName = await Application.Current.MainPage.DisplayPromptAsync("Збереження", "Введіть назву:", initialValue: fileName);
                if (string.IsNullOrWhiteSpace(inputName)) return;
                if (!inputName.EndsWith(".xml")) inputName += ".xml";
                string checkPath = Path.Combine(FileSystem.AppDataDirectory, inputName);
                if (File.Exists(checkPath)) { await Application.Current.MainPage.DisplayAlert("Помилка", $"Файл '{inputName}' вже існує.", "OK"); fileName = Path.GetFileNameWithoutExtension(inputName); }
                else { finalPath = checkPath; fileName = inputName; break; }
            }
            IsBusy = true;
            try
            {
                string xmlContent = GenerateXmlFromResults(SearchResultsList);
                await File.WriteAllTextAsync(finalPath, xmlContent);
                if (!ArchiveList.Contains(fileName)) ArchiveList.Add(fileName);
                SelectedArchive = fileName;
                await Application.Current.MainPage.DisplayAlert("Успіх", $"Архів збережено!", "OK");
            }
            catch (Exception ex) { await Application.Current.MainPage.DisplayAlert("Помилка", ex.Message, "OK"); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task SaveHtml()
        {
            if (string.IsNullOrEmpty(HtmlContent) || HtmlContent == DefaultHtmlContent || HtmlContent.Contains("Немає результатів"))
            {
                await Application.Current.MainPage.DisplayAlert("Увага", "Спочатку виконайте трансформацію в HTML.", "OK");
                return;
            }

            string fileName = "Report";
            string finalPath = "";

            while (true)
            {
                string inputName = await Application.Current.MainPage.DisplayPromptAsync("Збереження HTML", "Введіть назву файлу:", initialValue: fileName);
                if (string.IsNullOrWhiteSpace(inputName)) return;
                if (!inputName.EndsWith(".html")) inputName += ".html";

                string checkPath = Path.Combine(FileSystem.AppDataDirectory, inputName);
                if (File.Exists(checkPath))
                {
                    bool overwrite = await Application.Current.MainPage.DisplayAlert("Підтвердження", $"Файл '{inputName}' вже існує. Перезаписати?", "Так", "Ні");
                    if (overwrite) { finalPath = checkPath; break; }
                    fileName = Path.GetFileNameWithoutExtension(inputName);
                }
                else { finalPath = checkPath; break; }
            }

            try
            {
                await File.WriteAllTextAsync(finalPath, HtmlContent);
                await Application.Current.MainPage.DisplayAlert("Успіх", "HTML файл збережено у 📂", "OK");
            }
            catch (Exception ex) { await Application.Current.MainPage.DisplayAlert("Помилка", ex.Message, "OK"); }
        }

        [RelayCommand]
        private async Task DeleteCurrentArchive()
        {
            if (SelectedArchive == DefaultArchiveDisplayName) return;
            bool confirm = await Application.Current.MainPage.DisplayAlert("Видалення", $"Видалити '{SelectedArchive}'?", "Видалити", "Скасувати");
            if (!confirm) return;
            try
            {
                string path = Path.Combine(FileSystem.AppDataDirectory, SelectedArchive);
                if (File.Exists(path)) File.Delete(path);
                string removedName = SelectedArchive; SelectedArchive = DefaultArchiveDisplayName; ArchiveList.Remove(removedName);
                await Application.Current.MainPage.DisplayAlert("Успіх", "Архів видалено.", "OK");
            }
            catch (Exception ex) { await Application.Current.MainPage.DisplayAlert("Помилка", ex.Message, "OK"); }
        }

        [RelayCommand]
        private async Task Search()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                SearchResultsList.Clear(); SelectedMaterial = null; IsMaterialSelected = false;
                IsPlaceholderVisible = true; IsNoResultsFound = false; FoundCountText = "Пошук...";

                var criteria = new SearchCriteria
                {
                    Faculty = SelectedFaculty,
                    Department = SelectedDepartment,
                    AuthorFullName = AuthorQuery,
                    Title = TitleQuery
                };

                if (analysisStrategies.TryGetValue(SelectedAnalysisType, out var strategy))
                {
                    List<Material> results = await Task.Run(() => strategy.Analyze(criteria, loadedXmlContent));

                    if (results.Any())
                    {
                        await ApplySortAndDisplay(results);
                    }
                    else { IsNoResultsFound = true; FoundCountText = "Знайдено 0 співпадінь"; }
                }
            }
            catch (Exception ex) { await Application.Current.MainPage.DisplayAlert("Помилка", ex.Message, "OK"); }
            finally { IsBusy = false; }
        }

        private async Task ApplySortAndDisplay(List<Material> rawResults)
        {
            IEnumerable<Material> sorted = rawResults;
            if (SelectedSortOption == "За назвою (А-Я)") sorted = rawResults.OrderBy(x => x.Title);
            else if (SelectedSortOption == "За автором (А-Я)") sorted = rawResults.OrderBy(x => x.Author.FullName);
            else if (SelectedSortOption == "За датою (Нові)") sorted = rawResults.OrderByDescending(x => x.CreationDate);

            var sortedList = sorted.ToList();
            if (SearchResultsList.Count > 0) SearchResultsList.Clear();
            FoundCountText = $"Знайдено {sortedList.Count}. Відображення...";

            int batchSize = 10; int count = 0;
            foreach (var m in sortedList)
            {
                SearchResultsList.Add(m);
                count++;
                if (count % batchSize == 0) await Task.Delay(1);
            }
            FoundCountText = $"Знайдено {sortedList.Count} співпадінь";
        }

        [RelayCommand]
        private void ShowDetails(Material? material)
        {
            if (material != null) { SelectedMaterial = material; IsMaterialSelected = true; IsPlaceholderVisible = false; }
        }

        [RelayCommand]
        private async Task Transform()
        {
            if (IsBusy) return; IsBusy = true;
            try
            {
                if (!SearchResultsList.Any()) { HtmlContent = $@"<html style=""{BaseHtmlStyle}""><body><h3 style='font-weight:400;'>Немає результатів.</h3></body></html>"; return; }
                HtmlContent = $@"<html style=""{BaseHtmlStyle}""><body><h3>Трансформація...</h3></body></html>";
                string html = await Task.Run(() => {
                    var xslt = new XslCompiledTransform();
                    using var xslReader = XmlReader.Create(new StringReader(xslContent)); xslt.Load(xslReader);
                    string xml = GenerateXmlFromResults(SearchResultsList);
                    using var xmlReader = XmlReader.Create(new StringReader(xml));
                    using var sw = new StringWriter(); using var hw = XmlWriter.Create(sw, xslt.OutputSettings);
                    xslt.Transform(xmlReader, null, hw); return sw.ToString();
                });
                HtmlContent = html;
            }
            catch (Exception ex) { HtmlContent = $@"<html style=""{BaseHtmlStyle} color:red;""><body><h1>Помилка</h1><pre>{ex.Message}</pre></body></html>"; }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task Clear()
        {
            if (IsBusy) return; IsBusy = true;
            try
            {
                AuthorQuery = string.Empty;
                TitleQuery = string.Empty;
                if (Faculties.Any()) SelectedFaculty = Faculties.First();
                if (Departments.Any()) SelectedDepartment = Departments.First();
                SearchResultsList.Clear(); SelectedMaterial = null; IsMaterialSelected = false;
                IsPlaceholderVisible = true; IsNoResultsFound = false; FoundCountText = "";
                HtmlContent = DefaultHtmlContent;
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task Exit()
        {
            if (Application.Current?.MainPage != null && await Application.Current.MainPage.DisplayAlert("Вихід", "Завершити роботу?", "Так", "Ні")) Application.Current.Quit();
        }
    }
}