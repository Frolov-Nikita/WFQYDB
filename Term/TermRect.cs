namespace Term
{
    public class TermRect
    {
        private int width;
        private int height;
        private int x1;
        private int y1;

        public int X0 { get; set; }
        public int Y0 { get; set; }
        public int Width 
        {
            get => width;
            set 
            {
                width = value;
                if(width < 0)
                {
                    var tmp = X0;
                    X0 = X0 + width;
                    X1 = tmp;
                }
                x1 = X0 + width;
            }
        }
        public int Height 
        { 
            get => height;
            set
            {
                height = value;
                if (height < 0)
                {
                    var tmp = Y0;
                    Y0 = Y0 + width;
                    Y1 = tmp;
                }
                y1 = Y0 + height;
            }
        }
        public int X1 
        { 
            get => x1;
            set
            {
                x1 = value;
                if(x1 < X0)
                {
                    var tmp = X0;
                    X0 = x1;
                    x1 = tmp;
                }
                width = x1 - X0;
            }
        }
        public int Y1 
        { 
            get => y1; 
            set
            {
                y1 = value;
                if (y1 < Y0)
                {
                    var tmp = Y0;
                    Y0 = y1;
                    y1 = tmp;
                }
                height = y1 - Y0;
            }
        }

        public TermRect Add(TermRect rect)
        {
            if(rect != null)
            {
                X0 = min(X0, rect.X0);
                X1 = max(X1, rect.X1);

                Y0 = min(Y0, rect.Y0);
                Y1 = max(Y1, rect.Y1);
            }            

            return this;
        }

        int max(int a, int b) => a > b ? a : b;

        int min(int a, int b) => a < b ? a : b;
    }
}
