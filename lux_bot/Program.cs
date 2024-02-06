using System;
using System.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace ConsoleApp1
{
    class Program
    {
        private static long waitingForPhotoFromChatId = 0;
        private static string usersSelectedProcessingMethod = "0";
        static void Main(string[] args)
        {
            var client = new TelegramBotClient("6753716719:AAHROXUcs9FdUOS-j4hW5Re2N__Mlw9sfyI");
            client.StartReceiving(Update, Error);
            Console.ReadLine();
        }
     
        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;
            if (message != null)
            {
                if (message.Text != null)
                {
                    Console.WriteLine($"Получено новое сообщение от {message.Chat.FirstName}: {message.Text}");

                    if (message.Text == "/start")
                    {
                        var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
                        {
                        new[]
                        {
                            new KeyboardButton("Сделать фото черно-белым"),
                            new KeyboardButton("Хз")
                        },
                        new[]
                        {
                            new KeyboardButton("Что здесь "),
                            new KeyboardButton("Будет ")
                        }
                    })
                        {
                            ResizeKeyboard = true
                        };
                        var channelLink = "https://t.me/luxovie";
                        var inlineKeyboardMarkup = new InlineKeyboardMarkup(
                        new[]
                        {
                          InlineKeyboardButton.WithUrl("Luxовые сучки", channelLink)
                    });
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Приветствую!", replyMarkup: replyKeyboardMarkup);
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Сделайте свой выбор и не забудьте подписаться на наш тг канал!", replyMarkup: inlineKeyboardMarkup);

                    }
                    else if (message.Text.ToLower().Contains("я люксовая сучка"))
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Привет, красотка!");
                        return;
                    }
                if (message.Text.ToLower().Contains("сделать фото черно-белым"))
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Отправьте фото!");
                        usersSelectedProcessingMethod = "BW";

                    }
                }
                }

            if (message.Type == MessageType.Photo)
            {
                Console.WriteLine($"Получено фото от {message.Chat.FirstName}");

                var fileId = update.Message.Photo.LastOrDefault()?.FileId;
                var fileInfo = await botClient.GetFileAsync(fileId);
                var filePath = fileInfo.FilePath;

                // Получаем выбранный пользователем метод обработки фото
                var selectedProcessingMethod = usersSelectedProcessingMethod;

                // Обработка фото в соответствии с выбранным методом
                if (selectedProcessingMethod == "BW")
                {
                    string destinationFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{message.Photo[0].FileUniqueId}.png";
                    

                    await using (Stream fileStream = System.IO.File.Create(destinationFilePath))
                    {
                        var file = await botClient.GetFileAsync(message.Photo.LastOrDefault()?.FileId);
                        await botClient.DownloadFileAsync(file.FilePath, fileStream);
                    }
                   
                    string newFilePath = destinationFilePath.Replace(".png", "_edited.png");
                   
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(destinationFilePath))
                    {   
                        image.Mutate(x => x.Grayscale()); // Применяем черно-белый фильтр

                        var jpegEncoder = new JpegEncoder
                        {
                            Quality = 100 // Установка уровня качества сохранения в 100
                        };
                        image.Save(newFilePath, jpegEncoder);
                    }
                    using (Stream photoStream = System.IO.File.OpenRead(newFilePath))
                    {
                        await botClient.SendPhotoAsync(message.Chat.Id, photoStream);
                    }

                    System.IO.File.Delete(destinationFilePath);
                    System.IO.File.Delete(newFilePath);

                    await botClient.SendTextMessageAsync(message.Chat.Id, "Фото стало черно-белым!");
                }
                else if (selectedProcessingMethod == "Метод обработки 2")
                {
                    // Обработка фото по методу 2
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Фото успешно обработано по методу 2!");
                }

                // Сброс переменных
                waitingForPhotoFromChatId = 0;
                usersSelectedProcessingMethod="0";
            }
        }


                private static Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            var message = exception.Message;
            throw new Exception(message);
        }
    }
}