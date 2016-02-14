using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace WPFLocalizationExtensionDemoApplication.ViewModels.Examples
{
    public class GapTextWpfExampleViewModel : Screen
    {
        private string _city;
        private DayOfWeek _weekDay;
        private DateTime _openingTime;
        private DateTime _closingTime;
        private Animal _selectedAnimal;

        public GapTextWpfExampleViewModel()
        {
            base.DisplayName = "GapTextExample";
            this.City = "Berlin";
            this.WeekDay = DayOfWeek.Monday;

            this.Animals = new BindableCollection<Animal>();

            this.Animals.Add(new Animal("Dodo", DateTime.Now.Year - 1681, 1681));

            var million = 1000*1000;

            this.Animals.Add(new Animal("Stegosaurus", 195*million));
            this.Animals.Add(new Animal("Triceratops", 66*million));
            this.Animals.Add(new Animal("Diplodocus", 147*million));
            this.Animals.Add(new Animal("Apatosaurus", 145*million));

            this.SelectedAnimal = this.Animals.First();

            this.NotifyOfPropertyChange(() => this.Animals);
        }

        public string City
        {
            get { return _city; }
            set
            {
                if (value == _city) return;
                _city = value;
                NotifyOfPropertyChange(() => City);
            }
        }

        public DayOfWeek WeekDay
        {
            get { return _weekDay; }
            set
            {
                if (value == _weekDay) return;
                _weekDay = value;
                NotifyOfPropertyChange(() => WeekDay);
            }
        }

        public DateTime OpeningTime
        {
            get { return _openingTime; }
            set
            {
                if (value.Equals(_openingTime)) return;
                _openingTime = value;
                NotifyOfPropertyChange(() => OpeningTime);
            }
        }

        public DateTime ClosingTime
        {
            get { return _closingTime; }
            set
            {
                if (value.Equals(_closingTime)) return;
                _closingTime = value;
                NotifyOfPropertyChange(() => ClosingTime);
            }
        }

        public class Animal
        {
            public Animal(string name, int age, int? lastSeen = null)
            {
                this.Name = name;
                this.Age = age;
                this.LastSeen = lastSeen;
            }

            public string Name { get; set; }

            public int Age { get; set; }

            public int? LastSeen { get; set; }
        }

        public BindableCollection<Animal> Animals { get; set; }

        public Animal SelectedAnimal
        {
            get
            {
                return _selectedAnimal;
            }
            set
            {
                if (Equals(value, _selectedAnimal)) return;
                _selectedAnimal = value;
                NotifyOfPropertyChange(() => SelectedAnimal);
                NotifyOfPropertyChange(() => this.DynamicFormatString);
            }
        }

        public string DynamicFormatString
        {
            get
            {
                if (this.SelectedAnimal.LastSeen != null)
                {
                    return "The last {0} has been seen in {1}.";
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
