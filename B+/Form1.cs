using CodeExMachina;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace B_
{
    public partial class Form1 : Form
    {
        private class Connection
        {
            public Point StartPoint { get; set; }
            public Point EndPoint { get; set; }
        }

        private List<Connection> connections = new List<Connection>();
        public int count = 0;
        public List<string> allInfo = new List<string>();
        public List<string> info = new List<string>();
        BTree<IntWrapper> bTree = new BTree<IntWrapper>(2, new IntWrapperComparer());

        public Form1()
        {
            InitializeComponent();
            pictureBox1.Paint += PictureBox1_Paint;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1.HorizontalScrollbar = true;
            listBox1.ScrollAlwaysVisible = true;

            OpenFileDialog OPF = new OpenFileDialog();
            OPF.Multiselect = true;

            if (OPF.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(OPF.FileName))
                    {
                        while (!sr.EndOfStream)
                        {
                            listBox1.Items.Add(sr.ReadLine());
                        }
                    }
                    allInfo.Clear();

                    int numItems = listBox1.Items.Count;
                    count += numItems;
                    for (int i = 0; i < numItems; i++)
                    {
                        var item = (string)listBox1.Items[i];
                        allInfo.Add(item);

                    }
                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                        $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            saveFileDialog.Title = "Сохранить как";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                SaveTreeViewToFile(saveFileDialog.FileName);
            }
        }

        private void SaveTreeViewToFile(string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine($"{DateTime.Now}\nКоличество операций: {count}");
                // Вызываем рекурсивный метод для сохранения дерева в файл
                SaveNodeToFile(bTree.Root, sw, 0);
            }

            MessageBox.Show("Файл успешно сохранен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveNodeToFile(Node<IntWrapper> node, StreamWriter sw, int depth)
        {
            if (node == null)
                return;

            // Записываем информацию о текущем узле
            string nodeInfo = new string(' ', depth * 4) + $"Node: {string.Join(", ", node.Items)}";
            count++;
            sw.WriteLine(nodeInfo);
            count += node.Children.Length;
            // Рекурсивный вызов SaveNodeToFile для каждого дочернего узла
            for (int i = 0; i < node.Children.Length; i++)
            {
                SaveNodeToFile(node.Children[i], sw, depth + 1);
            }
        }

        private void DrawNode(Graphics g, Node<IntWrapper> node, int x, int y, int xOffset, int depth, Font font)
        {
            if (node == null)
                return;

            int verticalPadding = 60;  // Уменьшил вертикальный отступ
            int horizontalPadding = 40;  // Горизонтальный отступ

            string nodeInfo = $"Node: {string.Join(", ", node.Items)}";
            count++;
            SizeF textSize = g.MeasureString(nodeInfo, font);
            count++;
            RectangleF rect = new RectangleF(x - textSize.Width / 2, y, textSize.Width, textSize.Height);
            count += 3;
            g.FillRectangle(Brushes.LightGray, rect);
            g.DrawRectangle(Pens.Black, Rectangle.Round(rect));
            g.DrawString(nodeInfo, font, Brushes.Black, rect.Location);

            int nextY = y + (int)textSize.Height + verticalPadding;
            count += 3;
            count += node.Children.Length;
            for (int i = 0; i < node.Children.Length; i++)
            {
                int childX = x - (node.Children.Length - 1) * xOffset / 2 + i * xOffset;

                // Добавляем соединение в список
                connections.Add(new Connection
                {
                    StartPoint = new Point(x, y + (int)textSize.Height),
                    EndPoint = new Point(childX, nextY)
                });

                // Рекурсивный вызов DrawNode для дочернего узла
                DrawNode(g, node.Children[i], childX, nextY, xOffset, depth + 1, font);
            }
            count += 11;
        }


        // Измените ваш метод PictureBox1_Paint следующим образом
        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Font font = new Font("Arial", 10);
            int padding = 20;

            // Очищаем список соединений перед перерисовкой
            connections.Clear();

            // Вызываем метод отрисовки для корневого узла
            DrawNode(g, bTree.Root, ClientSize.Width / 2, padding, 200, 0, font);

            // Рисуем линии соединения после отрисовки узлов
            foreach (var connection in connections)
            {
                g.DrawLine(Pens.Black, connection.StartPoint, connection.EndPoint);
            }
            count += 11;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != null)
            {
                try
                {
                    if (int.TryParse(textBox1.Text, out int number))
                    {
                        IntWrapper intWrapper = new IntWrapper(number);
                        count++;
                        bTree.ReplaceOrInsert(intWrapper);

                        pictureBox1.Invalidate();

                        textBox1.Clear();
                    }
                    else
                    {
                        MessageBox.Show("Пожалуйста, введите корректное числовое значение.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private StringBuilder visualizationStringBuilder = new StringBuilder();

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count >= 1)
            {
                // Очищаем дерево перед добавлением новых элементов
                bTree.Clear(true);

                foreach (var item in allInfo)
                {
                    string[] numbers = item.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    count++;
                    foreach (string numberStr in numbers)
                    {
                        if (int.TryParse(numberStr, out int number))
                        {
                            IntWrapper intWrapper = new IntWrapper(number);
                            count++;
                            bTree.ReplaceOrInsert(intWrapper);
                        }
                        else
                        {
                            MessageBox.Show($"Ошибка преобразования строки в число: {numberStr}");
                        }
                    }
                }

                pictureBox1.Invalidate(); // Перерисовываем PictureBox
            }
            else
            {
                MessageBox.Show("Последовательность не была загружена!");
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            Graphics g = pictureBox1.CreateGraphics();
            Color backgroundColor = Color.White;
            count += 2;
            g.Clear(backgroundColor);
            connections.Clear(); // Очищаем также список точек соединения

            bTree.Clear(true);

            listBox1.Items.Clear();

            allInfo.Clear();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {

                if (comboBox1.SelectedIndex == 0 && int.TryParse(textBox1.Text, out int number))
                {
                    count++;
                    IntWrapper intWrapper = new IntWrapper(number);
                    // Вызываем метод DeleteItem для удаления элемента
                    IntWrapper deletedItem = bTree.DeleteItem(intWrapper, ToRemove.RemoveItem);
                    count++;
                    if (deletedItem != null)
                    {
                        MessageBox.Show($"Элемент {deletedItem} успешно удален из дерева.");
                        pictureBox1.Invalidate(); // Перерисовываем PictureBox после изменений в дереве
                    }
                    else
                    {
                        MessageBox.Show($"Элемент {intWrapper} не найден в дереве.");
                    }
                }
                else if (comboBox1.SelectedIndex == 1 )
                {
                    count++;
                    IntWrapper intWrapper = new IntWrapper(0);
                    IntWrapper deletedItem = bTree.DeleteItem(intWrapper, ToRemove.RemoveMin);
                    if (deletedItem != null)
                    {
                        MessageBox.Show($"Элемент {deletedItem} успешно удален из дерева.");
                        pictureBox1.Invalidate(); // Перерисовываем PictureBox после изменений в дереве
                    }
                    else
                    {
                        MessageBox.Show($"Элемент {intWrapper} не найден в дереве.");
                    }
                }
                else if (comboBox1.SelectedIndex ==2 )
                {
                    IntWrapper intWrapper = new IntWrapper(0);
                    IntWrapper deletedItem = bTree.DeleteItem(intWrapper, ToRemove.RemoveMax);
                    count += 3;
                    if (deletedItem != null)
                    {
                        MessageBox.Show($"Элемент {deletedItem} успешно удален из дерева.");
                        pictureBox1.Invalidate(); // Перерисовываем PictureBox после изменений в дереве
                    }
                    else
                    {
                        MessageBox.Show($"Элемент {intWrapper} не найден в дереве.");
                    }
                }
                else
                {
                    MessageBox.Show("Вы не ввели число или не выбрали опцию удаления!");
                }


                textBox1.Clear();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #region search
        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != null)
            {
                try
                {
                    if (int.TryParse(textBox1.Text, out int number))
                    {
                        IntWrapper intWrapper = new IntWrapper(number);

                        // Вызываем метод Get для поиска элемента в дереве
                        IntWrapper foundItem = bTree.Get(intWrapper);
                        count += 2;
                        if (foundItem != null)
                        {
                            MessageBox.Show($"Элемент {foundItem} найден в дереве.");
                        }
                        else
                        {
                            MessageBox.Show($"Элемент {intWrapper} не найден в дереве.");
                        }

                        textBox1.Clear();
                    }
                    else
                    {
                        MessageBox.Show("Пожалуйста, введите корректное числовое значение.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("О программе\n Программа преставляет собой операции над B+-деревом, такие как: добавление, удаление и добавление по одному числу.");
        }

        private void оВдеревеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("B+-дерево (англ. B+-tree) — структура данных на основе B-дерева, сбалансированное n-арное дерево поиска с переменным, но зачастую большим количеством потомков в узле. B+-деревья имеют очень высокий коэффициент ветвления (число указателей из родительского узла на дочерние, обычно порядка 100 или более), что снижает количество операций ввода-вывода, требующих поиска элемента в дереве.");
        }
    }
}
