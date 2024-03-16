namespace NorthwestCornerMethod
{
    // класс перевозка
    public class Shipment
    {
        // Кол-во вывезенного груза 
        public double Quantity { get; set; }

        // Стоимость перевозки
        public double CostPerUnit { get; }

        // номер строки в матрице распределения
        public int R { get; }
        // номер столбца в матрице распределения
        public int C { get; }

        // конструктор класса перевозка, в нем мы иницилизируем переданные значения
        public Shipment(double quantity, double costPerUnit, int r, int c)
        {
            Quantity = quantity;
            CostPerUnit = costPerUnit;
            R = r;
            C = c;
        }
    }
}