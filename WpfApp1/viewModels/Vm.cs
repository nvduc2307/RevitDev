namespace WpfApp1.viewModels
{
    public class Vm
    {
        public List<SettingModel> SettingModels { get; set; }
        public Vm()
        {
            SettingModels =
            [
                new SettingModel() { Name = "Name1", Projects = new List<string>() { "Model1", "Model2"} },
                new SettingModel() { Name = "Name2", Projects = new List<string>() { "Model1", "Model2", "Model3" } },
                new SettingModel() { Name = "Name3", Projects = new List<string>() { "Model1", "Model2", "Model3", "Model4" } },
                new SettingModel() { Name = "Name4", Projects = new List<string>() { "Model1", "Model2", "Model3", "Model4" } },
                new SettingModel() { Name = "Name4", Projects = new List<string>() { "Model1", "Model2", "Model3", "Model4" } },
                new SettingModel() { Name = "Name4", Projects = new List<string>() { "Model1", "Model2", "Model3", "Model4" } },
                new SettingModel() { Name = "Name4", Projects = new List<string>() { "Model1", "Model2", "Model3", "Model4" } },
                new SettingModel() { Name = "Name4", Projects = new List<string>() { "Model1", "Model2", "Model3", "Model4" } },
                new SettingModel() { Name = "Name4", Projects = new List<string>() { "Model1", "Model2", "Model3", "Model4" } },
                new SettingModel() { Name = "Name4", Projects = new List<string>() { "Model1", "Model2", "Model3", "Model4" } },
                new SettingModel() { Name = "Name4", Projects = new List<string>() { "Model1", "Model2", "Model3", "Model4" } },
                new SettingModel() { Name = "Name4", Projects = new List<string>() { "Model1", "Model2", "Model3", "Model4" } },
                new SettingModel() { Name = "Name4", Projects = new List<string>() { "Model1", "Model2", "Model3", "Model4" } },
                new SettingModel() { Name = "Name4", Projects = new List<string>() { "Model1", "Model2", "Model3", "Model4" } },
            ];
        }
    }
    public class SettingModel
    {
        public string Name { get; set; }
        public List<string> Projects {  get; set; }
    }
}
