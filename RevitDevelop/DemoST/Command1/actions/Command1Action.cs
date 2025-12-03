using Autodesk.Revit.UI;
using RevitDevelop.DemoST.Command1.models;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.RevFamilies;
using RevitDevelop.Utils.SkipWarning;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms;

namespace RevitDevelop.DemoST.Command1.actions
{
    public partial class Command1Action
    {
        private const string _extensionFa = ".rfa";
        private const string _extensionPr = ".rvt";
        private Command1Cmd _cmd;
        public Command1Action(Command1Cmd cmd)
        {
            _cmd = cmd;
        }
        public void Validate()
        {
            var isFamilyDocument = _cmd.Document.IsFamilyDocument;
            if (!isFamilyDocument) throw new Exception("Khong phai family");
        }
        public ObservableCollection<ProjectModel> LoadProject()
        {
            var docs = new ObservableCollection<ProjectModel>();
            var dialog = new OpenFileDialog();
            dialog.Filter = $"Revit files (*{_extensionPr})|*{_extensionPr}";
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var fileName in dialog.FileNames)
                {
                    var doc = new ProjectModel
                    {
                        IdGuid = "-1",
                        Name = fileName.Split('\\').LastOrDefault().Split('.').FirstOrDefault(),
                        FullPath = fileName,
                        IsSelected = true
                    };
                    docs.Add(doc);
                }
            }
            return docs;
        }
        public ObservableCollection<ProjectModel> GetProjectModels()
        {
            var docs = new ObservableCollection<ProjectModel>();
            var famDoc = _cmd.Document;
            foreach (var item in _cmd.Application.Documents)
            {
                if (item is not Document document)
                    continue;
                if (document.IsFamilyDocument)
                    continue;
                var doc = new ProjectModel
                {
                    IdGuid = document.ProjectInformation.UniqueId,
                    Name = document.Title,
                    FullPath = document.PathName,
                    IsSelected = true
                };
                docs.Add(doc);
            }
            return docs;
        }
        public void LoadFamilyIntoProject(ObservableCollection<ProjectModel> documentTargets)
        {
            var documentCurrents = _getDocumentCurrents();
            var famDoc = _cmd.Document;
            string tempRfa = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            (string.IsNullOrWhiteSpace(famDoc.Title) ? "Family" : famDoc.Title));
            if (!File.Exists(tempRfa))
                famDoc.SaveAs(tempRfa);
            var opts = new AlwaysOverwriteLoadOpts();
            foreach (var documentPath in documentTargets)
            {
                if (!documentPath.IsSelected)
                    continue;
                try
                {
                    var doc = _cmd.Application.OpenDocumentFile(documentPath.FullPath);
                    using (var ts = new Transaction(doc, "load family"))
                    {
                        ts.SkipAllWarnings();
                        ts.Start();
                        if (!doc.IsModifiable)
                            continue;
                        doc.LoadFamily(tempRfa, opts, out _);
                        ts.Commit();
                    }
                    doc.Save();
                    if (!documentCurrents.Any(x => x.ProjectInformation.Id == doc.ProjectInformation.Id))
                        doc.Close();
                }
                catch (Exception ex)
                {
                    IO.ShowException(ex);
                }
            }
            try { System.IO.File.Delete(tempRfa); } catch { }
        }
        private List<Document> _getDocumentCurrents()
        {
            var results = new List<Document>();
            var famDoc = _cmd.Document;
            foreach (var item in _cmd.Application.Documents)
            {
                if (item is not Document document)
                    continue;
                if (document.IsFamilyDocument)
                    continue;
                results.Add(document);
            }
            return results;
        }
    }
    public class AlwaysOverwriteLoadOpts : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        { overwriteParameterValues = true; return true; }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse,
            out FamilySource source, out bool overwriteParameterValues)
        { source = FamilySource.Family; overwriteParameterValues = true; return true; }
    }
}
