//using System.Globalization;
//using System.Windows;
//using Orcus.Shared.Connection;

//namespace Orcus.Administration.Plugins.Administration
//{
//    public abstract class AdministrationColumnInfo
//    {
//        public abstract string GetColumnName(CultureInfo cultureInfo);

//    }

//    public abstract class AdministrationTextColumnInfo : AdministrationColumnInfo
//    {
//        public abstract string GetColumnData(ClientInformation clientInformation);
//    }

//    public abstract class AdministrationDataTemplateColumnInfo : AdministrationColumnInfo
//    {
//        private DataTemplate _dataTemplate;

//        public DataTemplate DataTemplate => _dataTemplate ?? (_dataTemplate = GetDataTemplate());

//        protected abstract DataTemplate GetDataTemplate();
//    }
//}