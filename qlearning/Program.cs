using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace qlearning
{
    class Program
    {
        /***** değiştirilebilir alan  ****/
        public static List<char> line = new List<char> { 'X', '-', '-', '-', '-', '-', '-', '-', 'P', '-', '-', '-', '-', '-', '-', '-', 'G' };
        public static List<float> Qmatris = new List<float> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static List<int> Rmatris = new List<int> { -100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100 };
        public static float alpha = 0.5f; // öğrenme katsayısı 0-1
        public static int iteration = 5; // oyun sayısı
        public static int pointGoal = 5; // her oyunda ulaşılması gereken hedef
        public static int speedMS =50;// microsaniye cinsinden hız 1000ms=1s
        /*********************************/

        public static int initialState = 8; // playerın başlangıç noktası
        public static int actionState = 0;      // playerın gidebileceği eylemler
        public static int pointCurrent = 0; // bulanıldığı puan
        public static float lucky = 0.5f; //rastgele sağa sola gitme durumu
        public static string txtPerIterationQmatris=""; // her oyun sonunda elde edilen Q matrisin string hali
        public static List<List<string>> listOfResultIteration = new List<List<string>>(); //Her iterasyon sonunda bulunan sonuçları(adımsayısı,Qmatris) tutar.
        static void Main(string[] args)
        {
            Random rnd = new Random((int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond)); // ilerleyen işlemlerde kullanmak için milisaniye beslemelidir.(aynı sayıyı üretmemek için)

            for (int i = 0; i < iteration;i++) // oyun sayısı
            {
                List<string> resultIteration = new List<string>();
                //displayLine(line);
                bool result = true; // döngüden çıkma değişkeni
                int count = 0;// adım sayısı
                initialState = 8; // her oyun başladığında playerı başlangıç konumuna çekiyorum.
                pointCurrent = 0;
                Console.Clear();

                Console.Write("{0}. oyun başlıyor.",i+1);
                Thread.Sleep(3000);
                while (result) // false gelirse eğer döngüden çıkıyor . false getiren değer hedefi bulmasıdır.
                {
                    count++;
                    float num = (float)rnd.NextDouble(); 

                    if (num > lucky) // rastgele sağa ya da sola gitme durumu
                    {
                        actionState = chooseActionRandom(); 
                        Console.Write("rnd-> {0}'dan {1}'a hareket etti.\n", initialState, actionState);
                        if (initialState > actionState) result = shiftLEFT(); // sol tarafa hareket etme durumu
                        else result = shiftRIGHT(); // sağ tarafa hareket etme durumu
                    }
                    else // q değerine bakarak sağa ya da sola gitmeye karar verme durumu
                    {
                        actionState = chooseActionMax();
                        Console.Write("max-> {0}'dan {1}'a hareket etti.\n", initialState, actionState);
                        if (initialState > actionState) result = shiftLEFT();// sol tarafa hareket etme durumu
                        else result = shiftRIGHT();// sağ tarafa hareket etme durumu
                    }

                    qlearning();

                    if (pointCurrent != pointGoal) result = true; // o anki puan 
                    if (initialState == 0) { pointCurrent--; initialState = 8; } // sola ulaşmış ise -1
                    if (initialState == line.Count - 1) { pointCurrent++; initialState = 8; } // sağa ulaşma +1 değeri

                    if (pointCurrent == pointGoal || pointCurrent == pointGoal * -1) result = false; // her oyunun kaçta biteceğini bulma fonksiyonu

                    Console.Write("{0}. puan \n", pointCurrent); //puanı ekrana yazma

                    displayLine(line); // her speedMS kadar ekrana yazılan matris 

                    displayQmatris(); // her speedMS kadar zaman sonucunda hesaplanan Q matris

                    Thread.Sleep(speedMS);
                    Console.Clear();
                }//while

                //Console.Write("{0}", line.Count);  
                displayQmatris(); 
                resultIteration.Add(count.ToString()); // kaç adımda oyunu bitirdiğinin atamasını yapıyor
                resultIteration.Add(txtPerIterationQmatris); // oyun sonundaki Q matris değerini atama

                listOfResultIteration.Add(resultIteration); // tüm oyun değerlerini bir listeye atama
   
                Console.WriteLine();               
            }
            Console.Clear();

            lucky = (lucky + 1) / 2; // her seferinde şansını yüzde yüze yaklaştırma

            //iteration sonu
            displaySkorBoard();
            Console.ReadLine();
        }

        private static void displaySkorBoard() // bütün oyun değerlerinin sonuçlarını ekrana basma
        {
            int j=0;
            foreach (List<string> i in listOfResultIteration)
            {
                j++;
                List<string> resultIteration = i;
                Console.Write("{0}.oyun {1} adımda sonlandı.\n Durum matrisi: {2}\n", j, resultIteration[0], resultIteration[1]);
                //  esultIteration[0]=kaç adımda oyunu bitirdiğini yazıyor , resultIteration[1]=oyun sonundaki Q matris
            }

        }

        
        private static void qlearning()
        {
            if(actionState==line.Count-1) // listenin son durumundayken sadece sola gitme durumu
                 Qmatris[initialState] = alpha * (Rmatris[actionState] + Qmatris[actionState - 1]);
            else if(actionState==0)// listenin başındayken sadece sağa gitme durumu
                 Qmatris[initialState] = alpha * (Rmatris[actionState] + Qmatris[actionState + 1]);
            else if (Qmatris[actionState - 1] < 0 && Qmatris[actionState + 1]==0)
                Qmatris[initialState] = alpha * (Rmatris[actionState] + Qmatris[actionState - 1]); // - değerlerle dolsun olarakda q matrisi dolsun diye  (-50 0 0 0 0 0 0 0 0 0 0 50)
            else 
                Qmatris[initialState] = alpha * (Rmatris[actionState] + Math.Max(Qmatris[actionState - 1], Qmatris[actionState + 1]));
            // else için örnek q17=0.5*(r16+maq(q15,q17))  q=17=0.5* r18 + maq(q17, q19)
            //  listenin sonu için örnek  q19=0.5*(r20+maq(q19,q21)
        }

        private static int chooseActionMax() // Q matrisine bakarak karar verme
        {
            if (Qmatris[initialState + 1] == Qmatris[initialState - 1])
            {
                Random rnd = new Random((int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond));
                float num = (float)rnd.NextDouble();
                if (num > 0.5) actionState = initialState + 1;
                else actionState = initialState - 1;
            }
            else if (Qmatris[initialState + 1] > Qmatris[initialState - 1])
                actionState = initialState + 1;
            else
                actionState = initialState - 1;
            return actionState;
        }

        private static int chooseActionRandom() // %50 şans ile bulunduğu konumun bir öncesi  ya da bir sonrası durumuna gitmeyi atama
        {
            Random rnd = new Random((int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond));// rastgele gitmek için random sayı ürettim ona göre karar verdim.
            float num = (float)rnd.NextDouble();
            if (num > 0.5)
                actionState = initialState + 1;
            else
                actionState = initialState - 1;

            return actionState;
        }

       
        private static bool shiftLEFT()// playerı sol tarafa kaydırma fonksiyonu
        {
            if (initialState - 1 >=0)
            {
                for (int i = 0; i < line.Count; ++i)
                {
                    if (i == initialState - 1) line[i] = 'P';
                    else if (i == 0) line[i] = 'X';
                    else if (i == line.Count - 1) line[i] = 'G';
                    else line[i] = '-';
                }
                initialState--;          
            }
            if (initialState == 0) return false; // oyunu bitirme durumu
            return true;
        }
        private static bool shiftRIGHT() // playerı sağ tarafa kaydırma fonksiyonu
        {
            if (initialState + 1 < line.Count)
            {
                for (int i = 0; i < line.Count; ++i)
                {
                    if (i == initialState + 1) line[i] = 'P';
                    else if (i == 0) line[i] = 'X';
                    else if (i == line.Count - 1) line[i] = 'G';
                    else line[i] = '-';
                }
                initialState++;
            }
            if (initialState == line.Count) return false; // oyunu bitirme durumu
            return true;
        }

        private static void displayLine(List<char> line) // line dizisini her adımda ekrana yazma
        {       
            //Console.SetCursorPosition(0, 0);
            foreach (char i in line) 
                Console.Write("{0}", i.ToString());         
            Console.WriteLine();
            
        }
        private static void displayQmatris() //Q matrisin herhangi bir halini ekrana yazma
        {
            txtPerIterationQmatris = "";
            foreach (float i in Qmatris)
            {
                Console.Write("{0} ", i.ToString());
                txtPerIterationQmatris += " " + String.Format("{0:0.0000}", i.ToString());
            }
            Console.WriteLine();
        }

    }
}
