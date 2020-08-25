using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UpdateDb_Service.Models
{
    public class UpdateLogger:IDisposable
    {
        Stream logStream;
        public UpdateStats Stats { get; set; }

        private void LogWriteHeader(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes($"******************* {DateTime.Now:G} - {text} ******************\n");
            logStream.Write(data, 0, data.Length);
        }

        private void LogWriteChanges(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes($"  - {text}\n");
            logStream.Write(data, 0, data.Length);
        }
        private void LogWriteExceptionInfo(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes($"        - {text}\n");
            logStream.Write(data, 0, data.Length);
        }

        private void LogWriteInfo(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes($"{text}\n");
            logStream.Write(data, 0, data.Length);
        }

        private void LogWriteEmptyLine()
        {
            byte[] data = Encoding.UTF8.GetBytes("\n");
            logStream.Write(data, 0, data.Length);
        }


        public void ProcessUpdateStats()
        {
            string opBotMessage="";
            string adminBotMessage = "";

            //General
            if (Stats.OffersExtractions.Count > 0)
                LogWriteInfo($"{Stats.OffersExtractions.Where(oe => oe.IsSuccessful == true).Count()} of {Stats.OffersExtractions.Count} offers extractions were successfuly processed");
            if (Stats.PicturesExtractions.Count > 0)
                LogWriteInfo($"{Stats.PicturesExtractions.Where(oe => oe.IsSuccessful == true).Count()} of {Stats.PicturesExtractions.Count} pictures extractions were successfuly processed");
            if (Stats.DescriptionsExtractions.Count > 0)
                LogWriteInfo($"{Stats.DescriptionsExtractions.Where(oe => oe.IsSuccessful == true).Count()} of {Stats.DescriptionsExtractions.Count} descriptions extractions were successfuly processed");
            //Offers
            if (Stats.NewProductCategoriesAdded > 0)
            {
                opBotMessage += $"Категории товаров: {Stats.NewProductCategoriesAdded}\n";
                LogWriteChanges($"{Stats.NewProductCategoriesAdded} new match product categories were added");
            }
            if (Stats.NewVolumeTypesAdded > 0)
            {
                opBotMessage += $"Типы об./вес: {Stats.NewVolumeTypesAdded}\n";
                LogWriteChanges($"{Stats.NewVolumeTypesAdded} new match volume types were added");
            }
            if (Stats.NewVolumeUnitsAdded > 0)
            {
                opBotMessage += $"Ед. изм. об/веса: {Stats.NewVolumeUnitsAdded}\n";
                LogWriteChanges($"{Stats.NewVolumeUnitsAdded} new match volume units were added");
            }
            if (Stats.NewQuantityUnitsAdded > 0)
            {
                opBotMessage += $"Ед. изм. кол-ва: {Stats.NewQuantityUnitsAdded}\n";
                LogWriteChanges($"{Stats.NewQuantityUnitsAdded} new match quantity units were added");
            }
            if (Stats.NewExtraPropertiesAdded > 0)
            {
                opBotMessage += $"Доп. св-ва: {Stats.NewExtraPropertiesAdded}\n";
                LogWriteChanges($"{Stats.NewExtraPropertiesAdded} new match extra properties were added");
            }
            if (Stats.NewOffersAdded > 0)
            {
                opBotMessage += $"Товары: {Stats.NewOffersAdded}\n";
                LogWriteChanges($"{Stats.NewOffersAdded} new match offers were added");
            }

            if (Stats.MatchOffersDeleted > 0)
            {
                LogWriteChanges($"{Stats.MatchOffersDeleted} match offers were deleted");
            }
            if (Stats.ExistingOffersDeleted > 0)
            {
                LogWriteChanges($"{Stats.ExistingOffersDeleted} existing offers were deleted");
            }
            //Pictures
            if (Stats.NewUnmatchedPicsAdded > 0)
            {
                LogWriteChanges($"{Stats.NewUnmatchedPicsAdded} unmatched pictures were added");
            }
            if (Stats.NewMatchedPicsAdded > 0)
            {
                LogWriteChanges($"{Stats.NewMatchedPicsAdded} matched pictures were added");
            }
            if (Stats.NewConflictedPicsAdded > 0)
            {
                opBotMessage += $"Конфликтующие картинки: {Stats.NewOffersAdded}\n";
                LogWriteChanges($"{Stats.NewConflictedPicsAdded} conflicted pictures were added");
            }
            if (Stats.ProblemPics > 0)
            {
                LogWriteChanges($"{Stats.ProblemPics} were not processed because of problems");
            }
            //Descriptions
            if (Stats.NewUnmatchedDescsAdded > 0)
            {
                LogWriteChanges($"{Stats.NewUnmatchedDescsAdded} unmatched descriptions were added");
            }
            if (Stats.NewMatchedDescsAdded > 0)
            {
                LogWriteChanges($"{Stats.NewMatchedDescsAdded} matched descriptions were added");
            }
            if (Stats.NewConflictedDescsAdded > 0)
            {
                opBotMessage += $"Конфликтующие описания: {Stats.NewOffersAdded}\n";
                LogWriteChanges($"{Stats.NewConflictedDescsAdded} conflicted descriptions were added");
            }
            //Exceptions
            if (Stats.Exceptions.Count > 0)
            {
                LogWriteInfo("WARNING!!! EXCEPTIONS WERE THROWN DURING UPDATE PROCESS:");
                adminBotMessage += "<b>ВНИМАНИЕ!!!</b>\n<b>ПРИ ПОСЛЕДНЕЙ ЗАГРУЗКЕ БЫЛИ ВЫЗВАНЫ ИСКЛЮЧЕНИЯ:</b>\n";
                int count = 0;
                foreach (UpdateException e in Stats.Exceptions)
                {
                    count++;
                    LogWriteChanges($"{count}. \"{e.Message}\", CodeBlock:\"{e.CodeBlock}\", File:\"{e.FilePath}\"");
                    adminBotMessage += $"<b>{count}.</b> \"{e.Message}\"\n";
                    adminBotMessage += $"<b>CodeBlock:</b> \"{e.CodeBlock}\"\n";
                    adminBotMessage += $"<b>File:</b> \"{e.FilePath}\"\n";
                    if (e.InnerException != null)
                    {
                        LogWriteExceptionInfo($"Source: \"{e.InnerException.Source}\"");
                        adminBotMessage += $"<b>Source:</b> \"{e.InnerException.Source}\"\n";
                        LogWriteExceptionInfo($"Data: \"{e.InnerException.Data}\"");
                        adminBotMessage += $"<b>Data:</b> \"{e.InnerException.Data}\"\n";
                        LogWriteExceptionInfo($"Stack Trace: \"{e.InnerException.StackTrace}\"");
                        adminBotMessage += $"<b>Stack Trace:</b> \"{e.InnerException.StackTrace}\"\n";
                        LogWriteExceptionInfo($"Help Link: \"{e.InnerException.HelpLink}\"");
                        adminBotMessage += $"<b>Help Link:</b> \"{e.InnerException.HelpLink}\"\n\n";
                    }
                }
            }

            if (adminBotMessage != "")
            {
                AdminTGBot.SendMessageToAllAdmins(adminBotMessage);
            }
            if (opBotMessage !="")
            {
                opBotMessage = "<b>ВНИМАНИЕ! Появились несопоставленные позиции</b>\n"+ opBotMessage;
                OperatorTGBot.SendMessageToOperator(opBotMessage);
            }

        }

        public UpdateLogger(Uri logUri)
        {
            Stats = new UpdateStats();
            switch (logUri.Scheme)
            {
                case "file":
                    logStream = new FileStream(logUri.LocalPath, FileMode.Append, FileAccess.Write, FileShare.Read);
                    break;
                default:
                    break;
            }
            LogWriteHeader("Process of database update started");
        }

        public void Dispose()
        {
            LogWriteHeader("Process of database update finished");
            LogWriteEmptyLine();
            if (logStream != null)
            {
                logStream.Close();
                logStream.Dispose();
            }
        }
    }
}
