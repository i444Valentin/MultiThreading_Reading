using System;
using System.IO;
using System.Text;
using System.Threading;

namespace MultiThread_Reading
{
    class Program
    {
        private static readonly object __ObjectLock = new object(); //инициализируем объект-заглушку
        public static long count;
        static String path;

        static void Main(string[] args)
        {
            Console.WriteLine("Введите имя файла: ");
            path = Console.ReadLine();
            long length;
            using (FileStream fileM1Reader = File.OpenRead(path))
            {
                length = fileM1Reader.Length;
                Console.WriteLine("Длина строки: " + length);
                count = length / 4;
                Console.WriteLine("Длина частей: " + count);
            } 
            ClassFile clsFile = new ClassFile(); // объекты класса для каждого потока (определяет позицию курсора FileReader)
            ClassFile clsFile2 = new ClassFile();
            ClassFile clsFile3 = new ClassFile();
            ClassFile clsFile4 = new ClassFile();

            //определяем отступы
            clsFile.offset = 0;
            clsFile2.offset = count;
            clsFile3.offset = clsFile2.offset + count;
            clsFile4.offset = clsFile3.offset + count;
            clsFile4.count = count + (length % 4); // для последнего кол-во символов - + 0-3
            Console.WriteLine("Длина последней части: " + clsFile4.count);
            ClassFile[] arrayCLS = {clsFile, clsFile2,clsFile3,clsFile4};            
            for (int i = 0; i < 4; i++)
            {
                Thread threadFileRead = new Thread(new ParameterizedThreadStart(readTextLines)); //создаем поток с принимающим параметром
                threadFileRead.Name = "Поток " + i.ToString();
                threadFileRead.IsBackground = false; // потоки НЕ фоновые
                threadFileRead.Start(arrayCLS[i]);
                threadFileRead.Join(); //поток должен ожидать, пока другой поток закончит свою работу
            }          
        }


        private static void readTextLines(object obj)
        {
            lock (__ObjectLock) // устанавливаем объект-заглушку (чтобы другие потоки не начали свое выполнение, пока один поток выполняется)
            {
                ClassFile cls = (ClassFile)obj;
                using (FileStream reader = File.OpenRead(path))
                {
                    Byte[] textArray = new Byte[reader.Length];
                    UTF8Encoding temp = new UTF8Encoding(true);
                    reader.Seek(cls.offset, SeekOrigin.Begin); // устанавливаем курсор потока на позицию
                    if (reader.Read(textArray, 0, (int)cls.count) > 0)
                    {
                        String str = temp.GetString(textArray);
                        Console.WriteLine("Выводим содержимое файла: " + str + Thread.CurrentThread.Name); 
                        
                    }
                }
            }
        }
    }
}
