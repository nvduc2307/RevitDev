using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace RevitDevelop.Utils.Entities
{
    public class SchemaInfo
    {
        public string SchemaGuidID { get; }
        public string SchemaName { get; }
        public SchemaField SchemaField { get; }
        public Schema SchemaBase { get; }
        public SchemaInfo(string schemaGuidID, string schemaName, SchemaField schemaField)
        {
            SchemaGuidID = schemaGuidID;
            SchemaName = schemaName;
            SchemaField = schemaField;
            SchemaBase = CreateBaseSchema(SchemaGuidID, SchemaName, SchemaField);
        }
        public static Schema CreateBaseSchema(string schemaGuidID, string schemaName, SchemaField schemaField)
        {
            var schemaBuilder = new SchemaBuilder(new Guid(schemaGuidID));
            schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
            schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
            schemaBuilder.SetSchemaName(schemaName);
            schemaBuilder.AddSimpleField(schemaField.Name, typeof(string));
            var schema = Schema.Lookup(new Guid(schemaGuidID)) ?? schemaBuilder.Finish();// register the Schema object
            return schema;
        }

        public static Schema Write(Schema schemaBase, Element element, SchemaField schemaField)
        {
            try
            {
                if (element == null) return null;
                var entity = new Entity(schemaBase);
                var field = schemaBase.GetField(schemaField.Name);
                entity.Set(field, schemaField.Value);
                element.SetEntity(entity); // store the entity in the element
            }
            catch (Exception)
            {
            }
            return schemaBase;
        }

        public static SchemaField ReadAll(Schema schemaBase, SchemaField schemaField, Element element)
        {
            SchemaField results = null;
            if (schemaBase != null)
            {
                var field = schemaBase.GetField(schemaField.Name);
                if (field != null)
                {
                    var entity = element?.GetEntity(schemaBase);
                    if (entity != null && entity.IsValid())
                    {
                        results = new SchemaField()
                        {
                            Name = schemaField.Name,
                            Value = entity.Get<string>(field),
                        };
                    }
                }
            }
            return results;
        }
    }
}
