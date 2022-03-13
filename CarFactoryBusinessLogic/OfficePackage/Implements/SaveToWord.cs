using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarFactoryBusinessLogic.OfficePackage.HelperEnums;
using CarFactoryBusinessLogic.OfficePackage.HelperModels;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace CarFactoryBusinessLogic.OfficePackage.Implements
{
    public class SaveToWord : AbstractSaveToWord
    {
        private WordprocessingDocument wordDocument;
        private Body docBody;
        private static JustificationValues GetJustificationValues(WordJustificationType type)
        {
            return type switch
            {
                WordJustificationType.Both => JustificationValues.Both,
                WordJustificationType.Center => JustificationValues.Center,
                _ => JustificationValues.Left
            };
        }
        private static SectionProperties CreateSectionProperties()
        {
            var properties = new SectionProperties();
            var pageSize = new PageSize
            {
                Orient = PageOrientationValues.Portrait
            };
            properties.AppendChild(pageSize);
            return properties;
        }
        private static ParagraphProperties CreateParagraphProperties(WordTextProperties paragraphProperites)
        {
            if (paragraphProperites != null)
            {
                var properites = new ParagraphProperties();
                properites.AppendChild(new Justification() { Val = GetJustificationValues(paragraphProperites.JustificationType) });
                properites.AppendChild(new SpacingBetweenLines { LineRule = LineSpacingRuleValues.Auto });
                properites.AppendChild(new Indentation());
                var paragraphMarkRunProperties = new ParagraphMarkRunProperties();
                if (!string.IsNullOrEmpty(paragraphProperites.Size))
                {
                    paragraphMarkRunProperties.AppendChild(new FontSize { Val = paragraphProperites.Size });
                }
                properites.AppendChild(paragraphMarkRunProperties);
                return properites;
            }
            return null;
        }
        protected override void CreateWord(WordInfo info)
        {
            wordDocument = WordprocessingDocument.Create(info.FileName, WordprocessingDocumentType.Document);
            MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document();
            docBody = mainPart.Document.AppendChild(new Body());
        }
        protected override void CreateParagraph(WordParagraph paragraph)
        {
            if (paragraph != null)
            {
                var docParagraph = new Paragraph();
                docParagraph.AppendChild(CreateParagraphProperties(paragraph.TextProperties));
                foreach (var run in paragraph.Texts)
                {
                    var docRun = new Run();
                    var properties = new RunProperties();
                    properties.AppendChild(new FontSize { Val = run.Item2.Size });
                    if (run.Item2.Bold)
                    {
                        properties.AppendChild(new Bold());
                    }
                    docRun.AppendChild(properties);
                    docRun.AppendChild(new Text
                    {
                        Text = run.Item1,
                        Space = SpaceProcessingModeValues.Preserve
                    });
                    docParagraph.AppendChild(docRun);
                }
                docBody.AppendChild(docParagraph);
            }
        }
        protected override void SaveWord(WordInfo info)
        {
            docBody.AppendChild(CreateSectionProperties());
            wordDocument.MainDocumentPart.Document.Save();
            wordDocument.Close();
        }
        protected override void CreateTableWarehouses(WordInfo info)
        {
            Table table = new Table();

            TableProperties tblProps = new TableProperties(
                new TableBorders(
                new TopBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                },
                new BottomBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                },
                new LeftBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                },
                new RightBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                },
                new InsideHorizontalBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                },
                new InsideVerticalBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                }));

            table.AppendChild<TableProperties>(tblProps);
            docBody.AppendChild(table);
            TableRow tableRowHeader = new TableRow();
            TableCell cellHeaderName = new TableCell();
            cellHeaderName.Append(new TableCellProperties( new TableCellWidth() { Type = TableWidthUnitValues.Auto}));
            cellHeaderName.Append(new Paragraph(new Run(new Text("Название"))));
            TableCell cellHeaderResponsible = new TableCell();
            cellHeaderResponsible.Append(new TableCellProperties( new TableCellWidth() { Type = TableWidthUnitValues.Auto}));
            cellHeaderResponsible.Append(new Paragraph(new Run(new Text("ФИО ответственного"))));
            TableCell cellHeaderDateCreate = new TableCell();
            cellHeaderDateCreate.Append(new TableCellProperties( new TableCellWidth() { Type = TableWidthUnitValues.Auto}));
            cellHeaderDateCreate.Append(new Paragraph(new Run(new Text("Дата создания"))));
            tableRowHeader.Append(cellHeaderName);
            tableRowHeader.Append(cellHeaderResponsible);
            tableRowHeader.Append(cellHeaderDateCreate);
            table.Append(tableRowHeader);
            foreach(var warehouse in info.Warehouses)
            {
                TableRow tableRowWarehouse = new TableRow();
                TableCell CellWarehouseName = new TableCell();
                TableCell CellWarehouseResponsible = new TableCell();
                TableCell CellWarehouseDateCreate = new TableCell();
                CellWarehouseName.Append(new Paragraph(new Run(new Text(warehouse.WarehouseName))));
                CellWarehouseResponsible.Append(new Paragraph(new Run(new Text(warehouse.Responsible))));
                CellWarehouseDateCreate.Append(new Paragraph(new Run(new Text(warehouse.DateCreate.ToString()))));
                tableRowWarehouse.Append(CellWarehouseName);
                tableRowWarehouse.Append(CellWarehouseResponsible);
                tableRowWarehouse.Append(CellWarehouseDateCreate);
                table.Append(tableRowWarehouse);
            }
        }
    }
}
