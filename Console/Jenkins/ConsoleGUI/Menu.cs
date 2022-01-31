using System.Text;

namespace Jenkins.ConsoleGUI
{
    public class Menu
    {
        private int lasLen = 0;
        private bool canWork = true;
        private bool confimMode = false;
        private bool inputMode = false;
        private Item selected;

        public Item Main { get; }

        public Item Selected
        {
            get => selected;
            private set
            {
                selected = value;

                if (selected.ActionOnSelected)
                {
                    selected.Action?.Invoke().Wait();
                }
            }
        }

        public Item Current { get; private set; }
        public List<Item> Items { get; }
        public List<string> Log { get; } = new List<string>();

        public Menu() : this(string.Empty, Array.Empty<Item>())
        {
        }

        public Menu(Item[] items) : this(string.Empty, items)
        {
        }

        public Menu(string title, Item[] items)
        {
            Main = new Item(title, items);
            Items = new List<Item>(items);

            Current = Main;

            Selected = items.Length > 0 ? items[0] : Main;
        }

        public async Task BeginAsync()
        {
            Console.CursorVisible = false;

            while (canWork)
            {
                Refresh();

                escape: var info = Console.ReadKey(true);

                switch (info.Key)
                {
                    case ConsoleKey.Backspace:
                        {
                            if (Current.Parent != null)
                            {
                                foreach (var itm in Current.Parent.Items)
                                {
                                    if (itm == Current)
                                    {
                                        Selected = itm;

                                        break;
                                    }
                                }

                                Current = Current.Parent;
                            }

                            break;
                        }
                    case ConsoleKey.Escape:
                        {
                            Current = Main;

                            if (Main.Items.Count > 0)
                            {
                                Selected = Main.Items[0];
                            }

                            break;
                        }
                    case ConsoleKey.Enter:
                        {
                            if (Selected is InputItem)
                            {
                                inputMode = true;

                                break;
                            }


                            if (Selected.ActionIfConfirmed)
                            {
                                confimMode = true;

                                break;
                            }

                            await Selected.Action?.Invoke();

                            if (Selected.Items.Count > 0)
                            {
                                Current = Selected;
                                Selected = Current.Items[0];
                            }

                            break;
                        }
                    case ConsoleKey.UpArrow:
                        {
                            var sel = GetIndex();

                            if (sel > -1)
                            {
                                sel -= lasLen;

                                if (sel < 0)
                                {
                                    sel += Current.Items.Count;
                                }
                            }

                            Selected = Current.Items[sel];

                            break;
                        }
                    case ConsoleKey.DownArrow:
                        {
                            var sel = GetIndex();

                            if (sel > -1)
                            {
                                sel += lasLen;

                                if (sel >= Current.Items.Count)
                                {
                                    sel -= Current.Items.Count;
                                }
                            }

                            Selected = Current.Items[sel];

                            break;
                        }
                    case ConsoleKey.LeftArrow:
                        {
                            var sel = GetIndex();

                            if (sel > -1)
                            {
                                sel--;

                                if (sel < 0)
                                {
                                    sel = Current.Items.Count - 1;
                                }
                            }

                            Selected = Current.Items[sel];

                            break;
                        }
                    case ConsoleKey.RightArrow:
                        {
                            var sel = GetIndex();

                            if (sel > -1)
                            {
                                sel++;

                                if (sel == Current.Items.Count)
                                {
                                    sel = 0;
                                }
                            }

                            Selected = Current.Items[sel];

                            break;
                        }
                    case ConsoleKey.Delete:
                        {
                            Log.Clear();
                            break;
                        }
                    default:
                        {
                            if (confimMode)
                            {
                                if (info.Key == ConsoleKey.Y)
                                {
                                   await Selected.Action?.Invoke();
                                }

                                confimMode = false;

                                break;
                            }

                            goto escape;
                        }
                }
            }

            int GetIndex()
            {
                var sel = -1;

                for (var i = 0; i < Current.Items.Count; ++i)
                {
                    if (Selected == Current.Items[i])
                    {
                        sel = i;

                        break;
                    }
                }

                if (sel == -1)
                {
                    if (Current.Items.Count > 0)
                    {
                        sel = 0;
                    }
                }

                return sel;
            }
        }

        // Drawing
        public void Refresh()
        {
            if (inputMode)
            {
                var inp = Selected as InputItem;

                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Clear();
                Console.Write(inp.Title + ": ");

                Console.ResetColor();

                inp.Value = Console.ReadLine();

                inputMode = false;

                inp.Action?.Invoke(inp.Value);

                return;
            }

            if (confimMode)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine(Selected.Name.PadLeft(40));
                Console.WriteLine();
                //Console.WriteLine(ConfirmText.PadLeft(40));

                Console.ResetColor();

                return;
            }

            Console.Clear();

            // Drawing nav
            var nav = Current.Name;
            var cur = Current.Parent;
            var cursor = 0;

            while (cur != null)
            {
                nav = cur.Name + " => " + nav;

                cur = cur.Parent;
            }

            Console.WriteLine(nav);
            Console.WriteLine();

            var max_width = -1;

            for (var i = 0; i < Current.Items.Count; i++)
            {
                var itm = Current.Items[i];

                if (itm.Name.Length > max_width)
                {
                    max_width = itm.Name.Length;
                }
            }

            var col_space = 5;
            var len = Console.WindowWidth / (max_width + col_space) - 1;

            if (Current.MaxColumns > 0 && Current.MaxColumns < len)
            {
                len = Current.MaxColumns;
            }

            lasLen = len;

            for (var i = 0; i < Current.Items.Count; i += len)
            {
                for (var j = 0; j < len && i + j < Current.Items.Count; ++j)
                {
                    var itm = Current.Items[i + j];

                    if (itm == Selected)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }

                    var name = itm.Name.PadRight(max_width + 2);

                    if (name.Length >= Console.LargestWindowWidth)
                    {
                        name = name.Substring(0, Console.LargestWindowWidth - 5) + "...";
                    }

                    Console.Write(name);

                    Console.ResetColor();

                    Console.Write("".PadRight(col_space));
                }

                Console.WriteLine();

                var tmp = Console.CursorTop;

                if (tmp > cursor)
                {
                    cursor = tmp;
                }
            }

            Console.CursorTop = cursor + 1;
            Console.CursorLeft = 0;

            if (Log.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("______________________________");
                sb.AppendLine("");

                foreach (var itm in Log)
                {
                    sb.AppendLine(itm);
                }

                Console.Write(sb);
            }
        }

        //
        public void Close() => canWork = false;

        public void WriteLine(string str)
        {
            Log.Add(str);
            Refresh();
        }

        public class InputItem : Item
        {
            public new Action<string> Action { get; set; }
            public string Value { get; set; } = "";
            public string Title { get; set; }

            public InputItem(string name, string title, Action<string> action) : base(name, null as Func<Task>, 0)
            {
                Title = title;
                Action = action;
            }
        }

        public class Item
        {
            public Item Parent { get; private set; } = null;
            public string Name { get; }
            public Func<Task> Action { get; set; }
            public IReadOnlyList<Item> Items { get; }
            public object Tag { get; set; }
            public bool ActionOnSelected { get; set; } = false;
            public bool ActionIfConfirmed { get; set; } = false;
            public int MaxColumns { get; set; }

            public Item(string name, Func<Task> action, int maxColumns = 1) : this(name, action, new Item[0], maxColumns)
            {
            }

            public Item(string name, Item[] items, int maxColumns = 1) : this(name, null, items, maxColumns)
            {
            }

            public Item(string name, Func<Task> action, Item[] items, int maxColumns = 1)
            {
                Name = name;
                Action = action;
                MaxColumns = maxColumns;
                Items = new List<Item>(items);

                foreach (var itm in items)
                {
                    itm.Parent = this;
                }
            }

            public void Add(Item item)
            {
                item.Parent = this;

                ((List<Item>)Items).Add(item);
            }

            public Item Add(string name, Func<Task> a, int maxColumns = 0)
            {
                var itm = new Item(name, a, maxColumns) { Parent = this };

                ((List<Item>)Items).Add(itm);

                return itm;
            }

            public Item Add(string name, Func<Task> a, object tag, int maxColumns = 0)
            {
                var itm = new Item(name, a, maxColumns) { Parent = this, Tag = tag };

                ((List<Item>)Items).Add(itm);

                return itm;
            }

            public void Clear() => ((List<Item>)Items).Clear();
        }
    }
}
