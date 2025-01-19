using System.Collections.Generic;
using System.CommandLine;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Threading.Channels;

var rootCommand = new RootCommand("root command for File bundler cli");
var bundleCommand = new Command("bundle", "bundle code files to a single file ");
var createRspCommand = new Command("create-rsp", "create rsp file tdhet contain the long commend");

// הגדרת אופציות
var bundleOption = new Option<FileInfo>(new[] { "--output", "-o" }, "file path and name");
var languageOption = new Option<string>(new[] { "--language", "-l" }, "list programming languages") { IsRequired = true };
var noteOption = new Option<bool>(new[] { "--note", "-n" }, "write the destination as comment");
var sortOption = new Option<string>(new[] { "--sort", "-s" }, " copy of the files in sort mode");
var removeLinesOption = new Option<bool>(new[] { "--remove-empty-lines", "-r" }, " remove the empty lines from the file");
var authorOption = new Option<string>(new[] { "--author", "-a" }, " the name of the author as comment in the top of the file");



bundleCommand.AddOption(bundleOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeLinesOption);
bundleCommand.AddOption(authorOption);

//פונקציה לטיפול ב--output
void outputF(FileInfo output)
{
    try
    {
        File.Create(output.FullName).Dispose();
        Console.WriteLine($"File '{output.FullName}' was created.");
    }
    catch (DirectoryNotFoundException)
    {
        Console.WriteLine("Error: File path is invalid.");
    }
}


static void validInput(string languages)
{
    Dictionary<string, string> endFiles = getDict();
    languages = languages.ToUpper();
    if (languages == "ALL")
        return;
    if (languages.Contains(' '))
        throw new Exception("Error: The command cannot include spaces. Use commas between languages.");

    string[] languagesArray = languages.Split(',');
    foreach (string lang in languagesArray)
    {
        if (!endFiles.ContainsValue(lang))
            throw new Exception($"Error: The input '{lang}' is invalid.");
    }
}


static void BundleByLanguage(List<string> routes, string path, bool noteOption, bool removeLinesOption, string authorOption)
{
    // מוחק את הקובץ אם הוא כבר קיים כדי לא להתחיל כתיבה כפולה
    if (File.Exists(path))
    {
        File.Delete(path);
    }

    using (StreamWriter writer = new StreamWriter(path, append: true))
    {

        if (authorOption != null)
            writer.WriteLine("// " + authorOption);
        foreach (string route in routes)
        {
            if (noteOption)
                writer.WriteLine("// " + route);

            try
            {
                IEnumerable<string> content;
                if (removeLinesOption)
                    // קריאת השורות ושמירה רק על אלו שאינן ריקות
                    content = File.ReadLines(route).Where(line => !string.IsNullOrWhiteSpace(line));

                else
                    content = File.ReadLines(route);
                // כתיבה לקובץ החדש
                foreach (var line in content)
                {
                    writer.WriteLine(line);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {route}: {ex.Message}");
            }
        }
    }

    Console.WriteLine($"Files merged into '{path}' successfully.");
}
static Dictionary<string, string> getDict()
{
    Dictionary<string, string> endFiles = new Dictionary<string, string>
    {
        { ".cs", "C#" }, { ".js", "JAVASCRIPT" }, { ".java", "JAVA" },
        { ".py", "PYTHON" }, { ".cpp", "C++" }, { ".html", "HTML" },
        { ".css", "CSS" }, { ".ts", "TYPESCRIPT" }, { ".c", "C" },{ ".h","C++"}
    };
    return endFiles;
}
static void languageF(string language, string path, bool note, string sortOption, bool removeLinesOption, string authorOption)
{
    Dictionary<string, string> endFiles = getDict();
    try
    {
        validInput(language);
        string currentDirectory = Directory.GetCurrentDirectory();
        string[] files = Directory.GetFiles(currentDirectory);

        List<string> matchedFiles = new List<string>();
        language = language.ToUpper();
        foreach (string file in files)
        {
            string ext = Path.GetExtension(file);

            if (language == "ALL" || (endFiles.ContainsKey(ext) && language.Contains(endFiles[ext])) || (ext == ".h" && language.Contains("C")) || (ext == ".h" && language.Contains("C++")))
            {

                matchedFiles.Add(file);
            }
        }

        if (matchedFiles.Count == 0)
            Console.WriteLine("No files found for the specified languages.");
        else
        {
            if (sortOption == "order-by-type")
                matchedFiles = matchedFiles.OrderBy(route => Path.GetFileName(route)).ToList();
            else
            {
                if (sortOption != null&&sortOption != "order-by-name")
                {
                    Console.WriteLine("invalied sort");
                    return;
                }
                matchedFiles = matchedFiles.OrderBy(route => endFiles[Path.GetExtension(route)]).ToList();
            }
            BundleByLanguage(matchedFiles, path, note, removeLinesOption, authorOption);
        }

    }
    catch (Exception ex)
    {
        Console.WriteLine($"{ex.Message}");
    }
    return;

}

bundleCommand.SetHandler((FileInfo output, string language, bool noteOption, string sortOption, bool removeLinesOption, string authorOption) =>
{

    if (language == null)
    {
        Console.WriteLine("Error this reqierd field");
        return;
    }
    if (output != null)
    {
        outputF(output);
    }
    else
    {
        Console.WriteLine("enter the command output in order to export the file");
        return;
    }
    languageF(language, output.FullName, noteOption, sortOption, removeLinesOption, authorOption);

}, bundleOption, languageOption, noteOption, sortOption, removeLinesOption, authorOption);

createRspCommand.SetHandler(() =>
{

    Console.WriteLine("enetr the list of the languages that will be included in the bundle file");
    string languages = "";
    bool f = true;
    while (f)
    {
        try
        {
            languages = Console.ReadLine();
            validInput(languages);
            f = false; ;
        }
        catch (Exception e) { Console.WriteLine(e.Message); }
    }

    Console.WriteLine("enter the path of the bundle file");
    string path = Console.ReadLine();
    Console.WriteLine(" you want that the source link will be written?(y/n) ");
    char o = Char.Parse(Console.ReadLine());
    Console.WriteLine("enter :\n a if you want to sort by name ,\n b if yow want to sort by type\n");
    char s = Char.Parse(Console.ReadLine());
    Console.WriteLine(" you want to remove empty lines ?(y/n)");
    char r = Char.Parse(Console.ReadLine());
    Console.WriteLine("enter the name of the author and n if you dont want it to written");
    string nameAuthor = Console.ReadLine();

   File.Create("res.rsp").Dispose();
    using (StreamWriter rspWriter = new StreamWriter("res.rsp"))
    {
        string command = $" fib\n bundle\n --output {path}\n --language {languages}\n ";
        if (o == 'y')
            command += "--note \n";
        if (s == 'b')
            command += "--sort order-by-name\n";
        if (r == 'y')
            command += "--remove-empty-lines\n ";
        if (nameAuthor != "n")
            command += $"--author {nameAuthor}\n";
        rspWriter.Write( command);
        Console.WriteLine("the response file res.txt was created with succesfully ");
        Console.WriteLine($"the command is: {command}");
    }
}
);

rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);
await rootCommand.InvokeAsync(args);


