using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NorthwestCornerMethod
{
    class Program
    {
        // строка потребностей
        private static int[] _demand;
        // столбец запасов
        private static int[] _supply;
        // исходная матрица распределения
        private static double[,] _costs;

        private static string errorMessage = "С заданным условием задача неразрешима.";
        // Считываем данные из файла и инициализируем переменные
        // Считывается - строка потребностей, столбец запасов, матрица распределения
        // Помимо считывания, вводим дополнительную (фиктивную) потребность или запас
        private static void Init(string filename)
        {
            // объявляем считываемую строку
            string line;
            using (StreamReader file = new StreamReader(filename))
            {
                // Считываем из файла первую строку (кол-во строк и столбцов исходной матрицы)
                line = file.ReadLine();
                if (line == null)
                {
                    throw new Exception();
                }
                // Заполняем массив значениями из строки
                var numArr = line.Split();
                // Иницилизируем переменную первым элементом массива (кол-во строк)
                int countSupply = int.Parse(numArr[0]);
                // Иницилизируем переменную вторым элементом массива (кол-во столбцов)
                int countDemand = int.Parse(numArr[1]);

                // проверяем правильность введенных данных (если количество строк или столбцов равно нулю
                // то выводим ошибку)
                if (countSupply == 0 || countDemand == 0)
                {
                    throw new Exception();
                }

                // Считываем из файла вторую строку (значения исходной матрицы распределения)
                line = file.ReadLine();
                // Заполняем массив (одномерный) значениями из строки
                var costsArray = line.Split();

                // проверяем правильность введенных данных (если количество элементов
                // массива не соответствует количеству строк и столбцов то выводим ошибку)
                if (costsArray.Length != countDemand * countSupply)
                {
                    throw new Exception();
                }

                // Иницилизируем списки, которые будут принимать значения запасов и потребностей
                List<int> supplyList = new List<int>();
                List<int> demandList = new List<int>();

                // Считываем из файла третью строку (столбец запасов)
                line = file.ReadLine();
                // Заполняем массив (одномерный) значениями из строки
                numArr = line.Split();
                // Заполняем список значениями запасов
                for (int i = 0; i < countSupply; i++)
                {
                    supplyList.Add(int.Parse(numArr[i]));
                }

                // проверяем правильность введенных данных (если количество поставщиков
                // не равно количеству строк то выводим ошибку)
                if (supplyList.Count != countSupply)
                {
                    throw new Exception();
                }

                // Считываем из файла четвертую строку (строка потребностей)
                line = file.ReadLine();
                // Заполняем массив (одномерный) значениями из строки
                numArr = line.Split();
                // Заполняем список значениями потребностей
                for (int i = 0; i < countDemand; i++)
                {
                    demandList.Add(int.Parse(numArr[i]));
                }

                // проверяем правильность введенных данных (если количество потребителей
                // не равно количеству столбцов то выводим ошибку)
                if (demandList.Count != countDemand)
                {
                    throw new Exception();
                }

                // Вводим дополнительную потребность или запас.

                // Данное действие необходимо если модель исходной задачи
                // является открытой (нужно привести к закрытой модели).

                // В закрытой модели сумма потребностей равна сумме запасов.

                // Рассчитываем суммарное кол-во запасов.
                int totalSupply = supplyList.Sum();
                // Рассчитываем суммарную потребность
                int totalDemand = demandList.Sum();
                // Если суммарная потребность больше суммарного кол-во запасов
                if (totalSupply > totalDemand)
                {
                    // то добавляем вводим дополнительную (фиктивную) потребность.
                    demandList.Add(totalSupply - totalDemand);
                } // то же самое только с запасом
                else if (totalDemand > totalSupply)
                {
                    supplyList.Add(totalDemand - totalSupply);
                }
                // присваиваем массивам запасов и потребностей новые значения
                _supply = supplyList.ToArray();
                _demand = demandList.ToArray();

                // иницилизируем матрицу распределения
                // (дополнительный запас или потребность учитываются, так как мы уже изменили переменные)
                _costs = new double[_supply.Length, _demand.Length];

                // заполняем матрицу распределения значениями из файла 
                for (int i = 0; i < countSupply; i++)
                {
                    for (int j = 0; j < countDemand; j++)
                    {
                        // индекс считаем при этом перенося строку (i*количество столбцов + j)
                        _costs[i, j] = int.Parse(costsArray[i * (countDemand) + j]);
                    }
                }
            }
        }

        // Получение начального опорного плана методом северо западного угла
        private static Shipment[,] NorthWestCornerRule(int[] demand, int[] supply, double[,] costs)
        {
            // иницилизируем опорный план
            Shipment[,] plan = new Shipment[supply.Length, demand.Length];

            // копируем значения в локальные переменные
            // Массивы это ссылочные типы, поэтому чтобы переданные значения не изменялись вне метода
            // необходимо скопировать их в локальные переменные и изменять уже их.
            int[] currentDemand = (int[])demand.Clone();
            int[] currentSupply = (int[])supply.Clone();
            double[,] currentCosts = (double[,])costs.Clone();

            // Заполняем план с верхнего левого угла
            // Обходим матрицу распределения (элемент матрицы это перевозка)
            for (int r = 0, northwest = 0; r < currentSupply.Length; r++)
            {
                for (int c = northwest; c < currentDemand.Length; c++)
                {
                    // Смотрим по перевозке в матрице распределения
                    // Для перевозки есть число его запасов - currentSupply[r]
                    // и число его потребностей currentDemand[c]
                    // находим минимальное из них. Оно будет являться количеством вывезенного груза
                    // во время перевозки
                    int quantity = Math.Min(currentSupply[r], currentDemand[c]);

                    // Если мин. число = 0 (запас или потребность данного значения является фиктивной)
                    // то пропускаем данную перевозку (не вносим в план)
                    if (quantity > 0)
                    {
                        // Иницилизируем объект класса "перевозка"
                        plan[r, c] = new Shipment(quantity, currentCosts[r, c], r, c);

                        // вычитаем минимальный элемент (кол-во вывезененого груза) из значения запаса и потребности
                        currentSupply[r] -= quantity;
                        currentDemand[c] -= quantity;

                        // если значение запаса равно нулю 
                        if (currentSupply[r] == 0)
                        {
                            // то переходим на следующую строку
                            northwest = c;
                            break;
                        }
                    }
                }
            }

            // возвращаем план
            return plan;
        }

        // Получение плана в строковом виде
        private static string PlanToString(Shipment[,] planMatrix, int[] demand, int[] supply, double[,] costs, string titleMessage, string resultMessage)
        {
            string planString = titleMessage + "\n";
            // значение целевой функции опорного плана
            double totalCosts = 0;

            // запись первой строки плана (потребности)
            for (int i = 0; i < demand.Length; i++)
            {
                planString += demand[i] + " ";
            }
            planString += "\n";

            // обходим весь план
            for (int r = 0; r < supply.Length; r++)
            {
                for (int c = 0; c < demand.Length; c++)
                {
                    // запись первого столбца плана (запасы)
                    if (c == 0)
                    {
                        planString += supply[r] + " ";
                    }

                    // иницилизируем объект перевозки, для текущих индексов
                    Shipment s = planMatrix[r, c];

                    // если перевозка внесена в план
                    if (s != null && s.R == r && s.C == c)
                    {
                        if (s.Quantity == double.Epsilon)
                        {
                            planString += costs[r, c] +  " ";
                        }
                        else
                        {
                            // то записываем стоимость и количество вывезенного груза данной перевозки
                            planString += costs[r, c] + "|" + s.Quantity + " ";
                        }
                       
                        // прибавляем стоимость перевозки в целевую функцию
                        totalCosts += (s.Quantity * s.CostPerUnit);
                    } //если перевозка не внесена в план
                    else
                    {
                        // то записываем ее стоимость
                        planString += costs[r, c] + " ";
                    }
                }
                planString += "\n";
            }

            // вывод значения целевой функции опорного плана
            planString += "\n" + resultMessage + " F(x) равно " + totalCosts + "\n";

            return planString;
        }

        // Метод представляющий собой алгоритм потенциалов (метод потенциалов)
        // вовзращает оптимальный план
        private static Shipment[,] SteppingStone(Shipment[,] planMatrix, int[] demand, int[] supply, double[,] costs)
        {
            // наибольшое значение по абсолютной величине
            double maxReduction = 0;

            // перевозки
            Shipment[] move = null;
            // перевозка с наибольшим значением по абсолютной величине
            Shipment leaving = null;

            // объект оптимального плана
            Shipment[,] resultPlanMatrix = (Shipment[,])planMatrix.Clone();

            // устраняем вырожденность опорного плана
            FixDegenerateCase(resultPlanMatrix);

            // проверяем план на оптимальность 
            for (int r = 0; r < _supply.Length; r++)
            {
                for (int c = 0; c < _demand.Length; c++)
                {
                    // если перевозка есть в плане
                    if (resultPlanMatrix[r, c] != null)
                    {
                        // то завершаем шаг итерации
                        continue;
                    }

                    // создадим перевозку с грузом равным нулю
                    Shipment trial = new Shipment(0, _costs[r, c], r, c);
                    // возвращаем ациклический план
                    Shipment[] path = GetClosedPath(resultPlanMatrix, trial);

                    // сумма потенциалов
                    double reduction = 0;
                    // задаем максимальное значение, для того чтобы найти
                    // минимальное значения груза
                    double lowestQuantity = int.MaxValue;
                    // перевозка с минимальным значением груза
                    Shipment leavingCandidate = null;

                    
                    // отмечаем перевозку знаком плюс
                    bool plus = true;
                    // ищем максимальную значение по абсолютной величине
                    foreach (var s in path)
                    {
                        // если плюс то нам надо найти наибольшую перевозку
                        if (plus)
                        {               
                            // суммируем потенициал для этой перевозки
                            reduction += s.CostPerUnit;
                        } // если минус то находим наименьную
                        else
                        {
                            // вычитаем значение груза клетки
                            reduction -= s.CostPerUnit;

                            // ищем перевозку с минимальным значением груза
                            if (s.Quantity < lowestQuantity)
                            {
                                // запоминаем минимальную перевозку
                                leavingCandidate = s;
                                // и запоминаем минимальное значение груза
                                lowestQuantity = s.Quantity;
                            }
                        }
                        // поочередно меняем знак
                        plus = !plus;
                    }

                    // если сумма потенциалов (reduction) меньше значения
                    // максимального абсолютного значения
                    // то план не оптимальный
                    if (reduction < maxReduction)
                    {
                        // запоминаем цикл (план) при наибольшой абсолютной величине
                        move = path;
                        // запоминаем элемент с наибольшим значением по абсолютной величене
                        leaving = leavingCandidate;
                        // наибольшая абсолютная величина
                        maxReduction = reduction;
                    }
                }
            }

            // если план неоптимальный то перестроим его
            // если массив move имеет значения значит план неоптимальный
            // (массив заполнен перевозками разности которых меньше нуля)
            if (move != null)
            {
                // все что ниже это этап перераспределения поставок
                // запоминаем кол-во груза (минимальное значение груза)
                double q = leaving.Quantity;
                // отмечаем ячейку знаком плюс
                bool plus = true;
                // обходим все перевозки разности которых меньше нуля
                foreach (var s in move)
                {
                    // если true то вычитаем 
                    // если false то прибавляем
                    // прибавляем или вычитаем минимальное значение
                    s.Quantity += plus ? q : -q;
                    // записываем в план новое значение
                    // если новое кол-во груза равно нулю то эта перевозка null 
                    // в плане ее не учитываем
                    // если не равно нулю то добавляем ее в план
                    resultPlanMatrix[s.R, s.C] = s.Quantity == 0 ? null : s;
                    // меняем знак (будет меняться поочередно т.к. мы в цикле)
                    plus = !plus;
                }

                // пересчитываем оптимальный план
                resultPlanMatrix = SteppingStone(resultPlanMatrix, demand, supply, costs);
            }

            // возвращаем оптимальный план
            return resultPlanMatrix;
        }

        // метод преобразования плана в список (из двумерного массива в список)
        static List<Shipment> PlanToList(Shipment[,] plan)
        {
            // создаем объект списка
            List<Shipment> newList = new List<Shipment>();

            // заполняем список ненулевыми элементами массива
            foreach (var item in plan)
            {
                if (null != item)
                {
                    newList.Add(item);
                }
            }

            // возвращаем список
            return newList;
        }


        // возвращаем ациклический план
        // s - перевозка которой пополняем план
        // plan - план для пополнения
        static Shipment[] GetClosedPath(Shipment[,] plan, Shipment s)
        {
            // преобразуем план в список (из двумерного массива)
            List<Shipment> path = PlanToList(plan);
            // и добавляем в его конец перевозку
            path.Add(s);

            // удаляем (и продолжаем удалять) элементы
            // у которых нет соседа по горизонтали и по вертикали
            int before;
            do
            {
                // before - количество элементов до удаления
                before = path.Count;
                // удаляем все элементы удовлетворяющие условию
                path.RemoveAll(ship => 
                {
                    // nbrs - соседи элемента по вертикали и горизонтали (их 2)
                    var nbrs = GetNeighbors(ship, path);
                    // условие удаления
                    // если хотя бы один сосед не является перевозкой
                    // то удаляем элемент
                    return nbrs[0] == null || nbrs[1] == null;
                });
            }
            while (before != path.Count);

            // рассполагаем оставшиеся элементы в правильном порядке
            Shipment[] stones = path.ToArray();

            // объект перевозки которой пополняем план
            Shipment prev = s;
            // обходим список после удаления элементов 
            // все оставшиеся элементы составляют ациклический план
            for (int i = 0; i < stones.Length; i++)
            {
                // записываем в массив цикла перевозку
                stones[i] = prev;
                // записываем в значение prev ее соседа
                // для того чтобы потом записать его в цикл
                // возвращаем только соседей кратных двум
                prev = GetNeighbors(prev, path)[i % 2];
            }

            // ациклический план
            return stones;
        }

        // метод поиска соседей перевозки в плане
        static Shipment[] GetNeighbors(Shipment s, List<Shipment> lst)
        {
            // массив соседей перевозки
            Shipment[] nbrs = new Shipment[2];
            // ищем соседей по всему списку
            foreach (var o in lst)
            {
                // если перевозка которую обходим по списку
                // не является перевозкой для которую ищем соседей
                if (o != s)
                {
                    // то, если индекс строки перевозки в матрице распределения
                    // равен такому же индексу но для перевозки для которой ищем соседей 
                    // и при этом мы не нашли соседа для перевозки по строке
                    if (o.R == s.R && nbrs[0] == null)
                    {
                        // то записываем соседа в массив
                        nbrs[0] = o;
                    } // если индекс столбца перевозки в матрице распределения
                    // равен такому же индексу но для перевозки для которой ищем соседей 
                    // и при этом мы не нашли соседа для перевозки по столбцу
                    else if (o.C == s.C && nbrs[1] == null)
                    {
                        // то записываем соседа в массив
                        nbrs[1] = o;
                    }
                    
                    // если нашли двух соседей, то выходим из цикла foreach
                    if (nbrs[0] != null && nbrs[1] != null)
                    {
                        //выход из цикла foreach
                        break;
                    }
                }
            }

            // возвращаем соседей
            return nbrs;
        }

        // Метод возвращает ациклический план (базисные клетки не содержат циклов)
        // Метод сработает если план является невырожденым 
        // кол-во поставщиков + кол-во потребителей - 1.
        // если данное значение не равно количеству перевозок в плане, то план невырожденный
        static Shipment[,] FixDegenerateCase(Shipment[,] plan)
        {
            const double eps = double.Epsilon;
            
            // объект пополненного плана
            Shipment[,] fixedPlan = (Shipment[,])plan.Clone();

            // условие невырожденности
            if (_supply.Length + _demand.Length - 1 != PlanToList(plan).Count)
            {
                // проходим по элементам плана
                for (int r = 0; r < _supply.Length; r++)
                {
                    for (int c = 0; c < _demand.Length; c++)
                    {
                        // если перевозка отсутствует 
                        if (plan[r, c] == null)
                        {
                            // то ставим туда заглушку
                            // (перевозку с значением кол-во груза равным эпсилону)
                            // т.е. ставим нулевую перевозку в недостающие клетки
                            // и превращаем эти клетки в базисные
                            Shipment dummy = new Shipment(eps, _costs[r, c], r, c);

                            // Выбираем клетки для пополнения плана
                            // (GetClosedPath - возвращает массив представляющий собой цикл перевозок)
                            // если цикл перевозок равен нулю, то план является ациклическим при добавлении перевозки dummy
                            // значит можем добавлять
                            if (GetClosedPath(plan, dummy).Length == 0)
                            {
                                // ставим нулевую перевозку в недостающие клетки
                                plan[r, c] = dummy;
                                // возвращаем пополненный план
                                return fixedPlan;
                            }
                        }
                    }
                }
            }

            // возвращаем план
            return fixedPlan;
        }

        // основная функция программы (запускается первой)
        static void Main()
        {
            // название каталога
            var catalogName = @"C:\PR3\";
            // название входного файла
            var inputFileName = catalogName + "input.txt";
            // название файла с результатами
            var outputFileName = catalogName + "output.txt";

            try
            {
                // проверка на наличие каталога
                if (Directory.Exists(catalogName) == false)
                {
                    // если каталог отсутствует, то создаем его
                    Directory.CreateDirectory(catalogName);
                    // и создаем там файл
                    File.Create(inputFileName);
                    // выходим из программы, так как мы создали файл, но заполнить его должен пользователь
                    return;
                }

                // вызываем метод считывания данных из файла
                Init(inputFileName);
                // Вызываем метод реализующий алгоритм (Северо-западный угол)
                // получая тем самым объект опорного плана
                Shipment[,] firstPlanMatrix = NorthWestCornerRule(_demand, _supply, _costs);
                // преобразуем план в строковый формат
                string firstPlanString = PlanToString(firstPlanMatrix, _demand, _supply, _costs, "Исходная матрица:", "Исходное значение");

                // Вызываем метод реализующий алгоритм потенциалов (метод потенциалов)
                // получая тем самым объект оптимального плана
                Shipment[,] secondPlanMatrix = SteppingStone(firstPlanMatrix, _demand, _supply, _costs);
                // преобразуем план в строковый формат
                string secondPlanString = PlanToString(secondPlanMatrix, _demand, _supply, _costs, "Результативная матрица:", "Оптимальное значение");

                // вызываем метод для записи опорного и оптимального плана
                Out(outputFileName, firstPlanString + "\n" + secondPlanString);
            }
            catch (Exception)
            {
                // записываем сообщение об ошибке в файл
                Out(outputFileName, errorMessage);
            }
        }

        // метод записи ответа в файл
        private static void Out(string fileName, string result)
        {
            // проверка на наличие файла в директории
            if (File.Exists(fileName) == false)
            {
                // если нету то создаем
                File.Create(fileName);
            }

            // записываем результаты в файл (параметр false означает что если в файле
            // уже записаны какие либо данные то они будут удалены)
            using (StreamWriter file = new StreamWriter(fileName, false))
            {
                file.Write(result);
            }
        }
    }
}