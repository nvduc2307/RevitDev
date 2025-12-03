using Nice3point.Revit.Extensions;
using RevitDevelop.Utils.FilterElementsInRevit;
using RevitDevelop.Utils.Messages;
using System.IO;

namespace RevitDevelop.Utils.RevParameters
{
    public static class RevParameterUtils
    {
        public static List<RevParameter> GetAllParametersUserDefine(this Element element)
        {
            try
            {
                var parameters = element.GetOrderedParameters();
                var parametersUserDefine = parameters.Where(x =>
                {
                    return x.IsReadOnly ? false : x.Definition is InternalDefinition { BuiltInParameter: BuiltInParameter.INVALID };
                });
                return parametersUserDefine.Select(x => new RevParameter(x)).ToList();
            }
            catch (Exception)
            {
            }
            return new List<RevParameter>();
        }
        public static List<RevParameter> GetAllParameters(this Element element)
        {
            try
            {
                var parameters = element.GetOrderedParameters();
                var parametersUserDefine = parameters.Where(x =>
                {
                    return x.IsReadOnly ? false : x.Definition is InternalDefinition;
                });
                return parametersUserDefine.Select(x => new RevParameter(x)).ToList();
            }
            catch (Exception)
            {
            }
            return new List<RevParameter>();
        }
        public static List<RevParameter> GetAllTypeParameters(this ElementType elementType)
        {
            try
            {
                var parameters = elementType.GetOrderedParameters();
                var parametersUserDefine = parameters.Where(x =>
                {
                    return x.IsReadOnly ? false : x.Definition is InternalDefinition;
                });
                return parametersUserDefine.Select(x => new RevParameter(x)).ToList();
            }
            catch (Exception)
            {
            }
            return new List<RevParameter>();
        }
        public static bool HasParameter(this Element element, string parameterName)
        {
            if (element != null)
            {
                foreach (Parameter parameter in element.GetParameters(parameterName))
                {
                    if (parameterName == parameter.Definition.Name)
                    {
                        return true;
                    }
                }

                return false;
            }

            return false;
        }
        public static string GetParameterValue(this Element element, BuiltInParameter paraName)
        {
            try
            {
                var result = string.Empty;
                var para = element.get_Parameter(paraName);
                if (para == null) return result;
                var storageType = para.StorageType;
                switch (storageType)
                {
                    case StorageType.None:
                        break;
                    case StorageType.Integer:
                        result = para.AsInteger().ToString();
                        break;
                    case StorageType.Double:
                        result = para.AsDouble().ToString();
                        break;
                    case StorageType.String:
                        result = para.AsValueString();
                        break;
                    case StorageType.ElementId:
                        result = para.AsElementId().ToString();
                        break;
                }
                return result;

            }
            catch (Exception)
            {
            }
            return string.Empty;
        }
        public static string GetParameterValue(this Element element, string paraName)
        {
            var hasPara = HasParameter(element, paraName);
            if (!hasPara) return string.Empty;
            try
            {
                var result = string.Empty;
                var para = element.LookupParameter(paraName);
                var storageType = para.StorageType;
                switch (storageType)
                {
                    case StorageType.None:
                        break;
                    case StorageType.Integer:
                        result = para.AsInteger().ToString();
                        break;
                    case StorageType.Double:
                        result = para.AsDouble().ToString();
                        break;
                    case StorageType.String:
                        result = para.AsValueString();
                        break;
                    case StorageType.ElementId:
                        result = para.AsElementId().ToString();
                        break;
                }
                return result;

            }
            catch (Exception)
            {
            }
            return string.Empty;
        }
        public static void SetParameterValue(this Element element, string paraName, string paraValue)
        {
            try
            {
                var para = element.LookupParameter(paraName);
                if (para == null) return;
                var storageType = para.StorageType;
                switch (storageType)
                {
                    case StorageType.None:
                        break;
                    case StorageType.Integer:
                        para.Set(int.Parse(paraValue));
                        break;
                    case StorageType.Double:
                        para.Set(double.Parse(paraValue));
                        break;
                    case StorageType.String:
                        para.Set(paraValue);
                        break;
                    case StorageType.ElementId:
                        para.Set(new ElementId(int.Parse(paraValue)));
                        break;
                }

            }
            catch (Exception)
            {
            }
        }
        public static void SetParameterValue(this Element element, BuiltInParameter paraName, string paraValue)
        {
            try
            {
                var para = element.get_Parameter(paraName);
                var storageType = para.StorageType;
                switch (storageType)
                {
                    case StorageType.None:
                        break;
                    case StorageType.Integer:
                        para.Set(int.Parse(paraValue));
                        break;
                    case StorageType.Double:
                        para.Set(double.Parse(paraValue));
                        break;
                    case StorageType.String:
                        para.Set(paraValue);
                        break;
                    case StorageType.ElementId:
                        para.Set(new ElementId(int.Parse(paraValue)));
                        break;
                }

            }
            catch (Exception)
            {
            }
        }
        public static void DeleteParameter(this Document document, string nameParameter)
        {
            try
            {
                var par = document.GetElementsFromClass<ParameterElement>()
                        .FirstOrDefault(x => x.Name == nameParameter);
                if (par == null) return;
                document.Delete(par.Id);
            }
            catch (Exception)
            {
            }
        }
        public static void DeleteParameter(this Document document, List<string> nameParameters)
        {
            try
            {
                foreach (var namePara in nameParameters)
                {
                    var par = document.GetElementsFromClass<ParameterElement>()
                        .FirstOrDefault(x => x.Name == namePara);
                    if (par == null) continue;
                    document.Delete(par.Id);
                }
            }
            catch (Exception)
            {
            }
        }
        public static void DeleteParameter(
            this Document document,
            Autodesk.Revit.ApplicationServices.Application app,
            string pathShareParameter)
        {
            string originalFile = app.SharedParametersFilename;
            try
            {
                if (!File.Exists(pathShareParameter)) return;
                app.SharedParametersFilename = pathShareParameter;
                DefinitionFile sharedParameterFile = app.OpenSharedParameterFile();
                var parameterElements = document.GetElementsFromClass<ParameterElement>();
                foreach (DefinitionGroup dg in sharedParameterFile.Groups)
                {
                    var definitions = dg.Definitions;
                    foreach (var definition in definitions)
                    {
                        try
                        {
                            var parameterElement = parameterElements.FirstOrDefault(x => x.Name == definition.Name);
                            if (parameterElement == null) continue;
                            document.Delete(parameterElement.Id);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
            catch { }
            finally
            {
                app.SharedParametersFilename = originalFile;
            }
        }
        public static void CreateSharedParameters(
            Document doc,
            Autodesk.Revit.ApplicationServices.Application app,
            string pathShareParameter,
            BuiltInCategory cateToAdd)
        {
            string originalFile = app.SharedParametersFilename;
            try
            {
                Category category = doc.Settings.Categories.get_Item(cateToAdd);
                CategorySet categorySet = app.Create.NewCategorySet();
                categorySet.Insert(category);
                if (!File.Exists(pathShareParameter)) return;
                app.SharedParametersFilename = pathShareParameter;
                DefinitionFile sharedParameterFile = app.OpenSharedParameterFile();
                foreach (DefinitionGroup dg in sharedParameterFile.Groups)
                {
                    var definitions = dg.Definitions;
                    foreach (var definition in definitions)
                    {
                        try
                        {
                            ExternalDefinition externalDefinition_With = definition as ExternalDefinition;
                            //parameter binding 
                            InstanceBinding newIB = app.Create.NewInstanceBinding(categorySet);
                            //parameter group to text
#if REVIT2023 || REVIT2024 || REVIT2025
                            var dforgeid = new ForgeTypeId("autodesk.parameter.group:identityData-1.0.0");
                            doc.ParameterBindings.Insert(externalDefinition_With, newIB, dforgeid);
#else
                            doc.ParameterBindings.Insert(externalDefinition_With, newIB, BuiltInParameterGroup.PG_IDENTITY_DATA);
#endif
                        }
                        catch (Exception ex)
                        {
                            IO.ShowWarning(ex.Message);
                        }
                    }
                }
            }
            catch { }
            finally
            {
                app.SharedParametersFilename = originalFile;
            }
        }
    }
}
