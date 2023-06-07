using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGRModeling
{
    delegate void ModelAction(double time);
    delegate void BusAction(Bus bus);
    delegate void PeopleAction(People p);
    class World
    {
        public event ModelAction tickEvent;
        public List<double> timeLaps = new List<double>();
        public double globalTime = 0;
        public void AddNewTime(double time)
        {
            timeLaps.Add(time);
            timeLaps.Sort();
        }
        public void tick()
        {
            if (timeLaps.Count == 0)
                return;

            double time = timeLaps.FirstOrDefault();

            globalTime += time;
            for (int i = 0; i < timeLaps.Count; i++)
            {
                timeLaps[i] -= time;
                if (timeLaps[i] == 0)
                    timeLaps.RemoveAt(i--);
            }

            tickEvent(time);
        }
    }
    class People
    {
        Random rand = new Random();

        public event ModelAction NewAction;
        public event PeopleAction peopleComingEvent;
        double timer = 0;
        public double GetExpRandValue(double lyambda)
        {
            double m = Math.Pow(lyambda, -1);
            double x = rand.NextDouble();
            double y = -Math.Log(1 - x) / m;
            return y;
        }
        public void manComing()
        {
            double time = GetExpRandValue(1);
            timer = time;
            NewAction(time);
        }
        public void tick(double time)
        {
            timer -= time;
            if(timer<=0)
            {
                peopleComingEvent(this);
                manComing();
            }
        }
    }
    class Bus
    {
        public int size = 12;
        Random rand = new Random();
        public event ModelAction NewAction;
        public event BusAction busComingEvent;
        public double timer = 0;
        public double GetExpRandValue(double lyambda)
        {
            double m = Math.Pow(lyambda, -1);
            double x = rand.NextDouble();
            double y = -Math.Log(1 - x) / m;
            return y;
        }
        public void busComing()
        {
            double time = GetExpRandValue(10);
            timer = time;
            NewAction(time);
        }
        public void tick(double time)
        {
            timer -= time;
            if (timer <= 0)
            {
                busComingEvent(this);
                busComing();
            }
        }
    }

    class Stop
    {
        public int people;
        public int totalPeople;
        public int totalBus;
        public double totalwaitTime;
        public void tick(double time)
        {
            totalwaitTime += time * people;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            World w = new World();
            Stop s = new Stop();
            Bus b = new Bus();
            People p = new People();

            w.tickEvent += b.tick;
            w.tickEvent += p.tick;
            w.tickEvent += s.tick;
            p.NewAction += w.AddNewTime;
            p.peopleComingEvent+= delegate (People people) { s.people++; s.totalPeople++; };
            s.people += b.size;
            b.NewAction += w.AddNewTime;
            b.busComingEvent+= delegate (Bus bus) 
            { 
                if(s.people<bus.size)
                {
                    s.people = 0;
                }
                else
                {
                    s.people-=bus.size;
                }
                s.totalBus++;
            };

            Console.WriteLine("Введите количество минут:");
            int count = Convert.ToInt32(Console.ReadLine());

            b.tick(0);
            p.tick(0);
            while (w.globalTime < count)
            {
                w.tick();
            }

            Console.WriteLine("Среднее количество перевезёных людей за минуту: " + (int)( w.globalTime/ s.totalPeople));
            Console.WriteLine("Среднее количество перевезёных людей за час: " + (int)(w.globalTime / s.totalPeople*60) );
            Console.WriteLine("Среднее время ожидания для человека: " + (s.totalwaitTime/ s.totalPeople));
            Console.WriteLine("Автобус приезжает в среднем раз в: " + (w.globalTime/s.totalBus)+" минут");
            Console.WriteLine("Всего перевезено людей: " + s.totalPeople);
        }
    }
}
