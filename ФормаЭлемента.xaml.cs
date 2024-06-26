﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Cursovaya
{
    /// <summary>
    /// Логика взаимодействия для ФормаЭлемента.xaml
    /// </summary>
    public delegate void BackToElementForm(string Table, string PrimaryKeyValue);
    public partial class ФормаЭлемента : Window
    {
        public Dictionary<string, string> Element;
        public TableInfo TableInfo;
        Control PrimaryKeyField;
        DatePicker ДатаЗаписиВРегистр=null;
        private UpdateTable UpdateTableDelegate;
        private bool ThisIsNewElement;
        public ФормаЭлемента(Dictionary<string, string> element, TableInfo tableinfo, UpdateTable updatetable, bool thisisnewelement)
        {
            InitializeComponent();
            // если ThisIsNewElement то element содержит пустые значения
            Element = element;
            TableInfo = tableinfo;
            UpdateTableDelegate = updatetable;
            ThisIsNewElement = thisisnewelement;
            ElementTitle.Content = ThisIsNewElement ? "Создание записи" : "Редактирование записи" ;
            
            foreach (var ColumnName_Value in Element)
            {
                ColumnInfo ColumnInfo = TableInfo.ColumnName_ColumnInfo[ColumnName_Value.Key];

                Label label = new Label();
                label.Content = ColumnName_Value.Key.Replace("_","__") + ":";

                Реквизиты.Children.Add(label);
                Control Box;
                if (ColumnInfo.LinkedTableName != null)
                { 
                    ComboBox comboBox = new ComboBox();
                    comboBox.Text = ColumnName_Value.Value;
                    comboBox.Name = ColumnInfo.LinkedTableName;
                    comboBox.DropDownOpened += ComboBox_DropDownOpened;
                    Box = comboBox;
                }
                else if (ColumnInfo.Type == ColumnInfo.Types.DATE) 
                {
                    DateTime date;
                    DatePicker datePicker = new DatePicker();
                    if (DateTime.TryParseExact(
                        ColumnName_Value.Value, 
                        "yyyy-MM-dd H:mm:ss", 
                        CultureInfo.CurrentCulture, 
                        DateTimeStyles.None, 
                        out date))
                    {
                        datePicker.SelectedDate = date;
                    }
                    Box = datePicker;
                }
                else
                {
                    TextBox textBox = new TextBox();
                    textBox.Text = ColumnName_Value.Value;
                    Box = textBox;
                }


                if (ColumnInfo.IsItPrimaryKey)
                {
                    PrimaryKeyField = Box;
                    if (ThisIsNewElement && Box is TextBox textBox && ColumnInfo.Type==ColumnInfo.Types.INTEGER)
                        textBox.Text = db.GetAutoIncrement(TableInfo.TableName, ColumnInfo.ColumnName).ToString();
                }

                // Нельзя менять PrimaryKey если элемент изменяется, но можно его задать при создании
                Box.IsEnabled = ThisIsNewElement || !ColumnInfo.IsItPrimaryKey;
                ЗначенияРеквизитов.Children.Add(Box);
            }
            
            if (TableInfo.СвязанныйРегистр != null)
            {
                DatePicker datePicker = new DatePicker();
                datePicker.SelectedDate = DateTime.Now;
                Label label = new Label();
                label.Content = "Дата записи в регистр:";
                ДатаЗаписиВРегистр = datePicker;
                Реквизиты.Children.Add(label);
                ЗначенияРеквизитов.Children.Add(datePicker);
            }
        }


        private void Write(bool CLoseAfter=false)
        {
            bool Отказ = false;
            bool Истина = true;

            // Изменение строки
            string UpdateQuery = $"UPDATE {TableInfo.TableName} SET ";
            string Where = "WHERE ";

            // Добавление строки
            string AddQuery = $"INSERT INTO {TableInfo.TableName}({string.Join(", " , Element.Keys)}) VALUES ";


            Dictionary<string, string> NewElement = new Dictionary<string, string>();

            for (int i=0; i < Element.Count; i++)
            {
                var CurrenElement = Element.ElementAt(i);
                ColumnInfo ColumnInfo = TableInfo.ColumnName_ColumnInfo[CurrenElement.Key];

                Control CurrentBox = (Control)ЗначенияРеквизитов.Children[i];
                string CurrentValue;

                if (CurrentBox is TextBox textBox)
                    CurrentValue = textBox.Text;
                else if (CurrentBox is ComboBox comboBox)
                    CurrentValue = comboBox.Text;
                else if (CurrentBox is DatePicker dateBox)
                    CurrentValue = dateBox.SelectedDate == null ? string.Empty : ((DateTime)dateBox.SelectedDate).ToShortDateString();
                else
                    continue;


                // Обновление данных Element
                NewElement.Add(ColumnInfo.ColumnName,CurrentValue);

                // Запрос изменения. Primary key нельзя обновлять, но нужно указывать в Where
                bool NextIsPrimaryKey_AndThisIsEnd = ((i+1)+1 == Element.Count) && TableInfo.ColumnName_ColumnInfo.ElementAt(i+1).Value.IsItPrimaryKey;
                bool CurrentIsEnd = i + 1 == Element.Count;
                if (!ColumnInfo.IsItPrimaryKey)
                    UpdateQuery += $"{ColumnInfo.ColumnName} = '{CurrentValue}'{( CurrentIsEnd || NextIsPrimaryKey_AndThisIsEnd ? "" : ",")} ";

                // Если элемент новый то старой даты указано не будет, кроме того запрос обновления не увидит свет
                string OldValue = CurrenElement.Value;
                if (ColumnInfo.Type == ColumnInfo.Types.DATE && !ThisIsNewElement)
                {
                    DateTime OldDate;
                    if (DateTime.TryParseExact(
                            OldValue,
                            "yyyy-MM-dd H:mm:ss",
                            CultureInfo.CurrentCulture,
                            DateTimeStyles.None,
                            out OldDate) ||
                        DateTime.TryParseExact(
                            OldValue,
                            "yyyy-MM-dd",
                            CultureInfo.CurrentCulture,
                            DateTimeStyles.None,
                            out OldDate))
                        OldValue = OldDate.ToShortDateString();
                    else
                    {
                        //Отказ = Истина;
                        MessageBox.Show("Что то не то со старой датой");
                        //continue;
                    }
                }
                Where += $"{ColumnInfo.ColumnName} = '{OldValue}'{( CurrentIsEnd ? "" : " AND ")} ";


                if (CurrentValue==string.Empty && ColumnInfo.NotNull)
                {
                    Отказ = Истина;
                    MessageBox.Show($"Поле {ColumnInfo.ColumnName} обязательно для заполнения");
                    continue;
                }
                else if ((!int.TryParse(CurrentValue, out int _)) && ColumnInfo.Type == ColumnInfo.Types.INTEGER)
                {
                    Отказ = Истина;
                    MessageBox.Show($"Поле {ColumnInfo.ColumnName} должно быть целым числом");
                }
                else if ((!double.TryParse(CurrentValue, out double _)) && ColumnInfo.Type == ColumnInfo.Types.REAL)
                {
                    Отказ = Истина;
                    MessageBox.Show($"Поле {ColumnInfo.ColumnName} должно быть рациональным числом");
                }
                else if (ColumnInfo.LinkedTableName != null)
                {
                    string LinkedTablePrimaryKey = TableInfo.LinkedTableName_LinkedTablePrimaryKey[ColumnInfo.LinkedTableName];
                    string check_query = $"SELECT 1 FROM {ColumnInfo.LinkedTableName} WHERE {LinkedTablePrimaryKey} = '{CurrentValue}'";
                    if (db.GetDataTableByQuery(check_query).Rows.Count == 0)
                    {
                        Отказ = Истина;
                        MessageBox.Show($"Поле {ColumnInfo.ColumnName} должно ссылаться на элемент таблицы {ColumnInfo.LinkedTableName}");
                    }
                }

            }
            
            // Завершение запроса изменения
            UpdateQuery += Where;

            // Завершение запроса добавления
            AddQuery += $"('{string.Join("', '", NewElement.Values)}')";


            string Query                = ThisIsNewElement ? AddQuery : UpdateQuery;
            TableUpdateTypes updateType = ThisIsNewElement ? TableUpdateTypes.INSERT : TableUpdateTypes.UPDATE;


            // Проверка заполнения даты записи в регистр в случае если это нужно
            string РесСвРег = TableInfo.РесурсСвязянногоРегистра;
            if (РесСвРег!=null)
            {
                if (ДатаЗаписиВРегистр.SelectedDate == null)
                {
                    if (Element[РесСвРег] != NewElement[РесСвРег] || ThisIsNewElement)
                    {
                        Отказ = Истина;
                        MessageBox.Show("Заполните дату записи в регистр");
                    }
                    // Иначе запись в регистр не требуется
                }
                else
                {
                    var RegInfo = db.GetTableInfo(TableInfo.СвязанныйРегистр);
                    string TableForeignKeyName = RegInfo.ColumnName_ColumnInfo.FirstOrDefault(pair => pair.Value.LinkedTableName == TableInfo.TableName).Key;

                    DateTime date = (DateTime)ДатаЗаписиВРегистр.SelectedDate;
                    string CheckQuery = "" +
                        $"SELECT 1 " +
                        $"FROM  {TableInfo.СвязанныйРегистр} " +
                        $"WHERE Дата = '{date.ToShortDateString()}' AND {TableForeignKeyName} = '{NewElement[TableInfo.PrimaryKey]}' ";

                    var result = db.GetDataTableByQuery(CheckQuery);
                    if (result.Rows.Count > 0) 
                    {
                        Отказ = Истина;
                        MessageBox.Show("На выбранную дату уже существует запись в регистре, выберите другую дату");
                    }
                }
                
            }

            if (Отказ)
                return;

            // Проверок сверху может не хватить
            try
            {
                int rowschanged = db.ExecuteNonQuery(Query);
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
                return;
            }

            // Запись изменения в регистр
            if (РесСвРег != null)
            {
                bool continueflag = true;
                
                if (ДатаЗаписиВРегистр.SelectedDate == null)
                    continueflag = false;
                
                
                else if (Element[РесСвРег] == NewElement[РесСвРег] && !ThisIsNewElement)
                // или !(Element[РесСвРег] != NewElement[РесСвРег] || ThisIsNewElement)
                {
                    string messageBoxText = $"Значние ресурса связанного регистра {РесСвРег} не изменилось, хотите создать запись в связанном регистре?";
                    string caption = "Word Processor";
                    MessageBoxButton button = MessageBoxButton.YesNo;
                    MessageBoxImage icon = MessageBoxImage.Warning;
                    MessageBoxResult result;

                    result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.No);
                    continueflag = result == MessageBoxResult.Yes;
                }

                if (continueflag)
                {
                    try
                    {
                        db.ДобавитьЗаписьВРегистр(
                            TableInfo.СвязанныйРегистр,
                            TableInfo.TableName,
                            NewElement[TableInfo.PrimaryKey],
                            РесСвРег,
                            NewElement[РесСвРег],
                            (DateTime)ДатаЗаписиВРегистр.SelectedDate);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при записи в связанный регистр: {ex.Message}, изменение не было записано в регистр");

                    }
                }

            }


            // После успешного завершения запроса, элемент уже не новый
            ThisIsNewElement = false;
            ElementTitle.Content = "Редактирование записи";
            if (TableInfo.PrimaryKey != null)
                PrimaryKeyField.IsEnabled = false;

            // Обновление данных
            Element = NewElement;

            // Обновлание таблицы в форме списка
            UpdateTableDelegate(updateType);
            this.Focus();

            if (CLoseAfter)
                this.Close();
        }

        private void Write_Click(object sender, RoutedEventArgs e)
        {
            Write();
        }
        private void WriteAndClose_Click(object sender, RoutedEventArgs e)
        {
            Write(true);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Choosed(string Table, string LinkedTablePrimaryKeyValue)
        {
            ComboBox comboBox = FindControlByName(Table) as ComboBox;
            comboBox.Text = LinkedTablePrimaryKeyValue;
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {

            ComboBox comboBox = sender as ComboBox;
            
            comboBox.IsDropDownOpen = false;

            string LinkedTableName = comboBox.Name;

            if (App.UserAccess[TableInfo.TableName] == AccessRights.НетДоступа)
            {
                MessageBox.Show($"У вас нет доступа к таблице {LinkedTableName}, связанной с этим полем");
                return;
            }

            string startPos = comboBox.Text;

            BackToElementForm del = new BackToElementForm(this.Choosed);

            ФормаВыбора ChooseForm = new ФормаВыбора(LinkedTableName, startPos, del);
            ChooseForm.ShowDialog();
        }

        public Control FindControlByName(string name)
        {
            foreach (Control control in ЗначенияРеквизитов.Children)
            {
                if (control.Name == name)
                {
                    return control;
                }
            }

            return null;
        }

    }
}
