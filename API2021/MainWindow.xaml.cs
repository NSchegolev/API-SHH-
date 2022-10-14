using System;
using System.Collections;
using System.Windows;
using Tekla.Structures.Model;
using Point = Tekla.Structures.Geometry3d.Point;
//using System.Windows.Forms;
using TS = Tekla.Structures;
using TSG = Tekla.Structures.Geometry3d;
using Tekla.Structures.Geometry3d;
using TSG3D = Tekla.Structures.Geometry3d;
using TSM = Tekla.Structures.Model;
using Tekla.Structures.Solid;
using TSMUI = Tekla.Structures.Model.UI;//для взаимодействия с пользователем
using Tekla.Structures.Model.UI;//для взаимодействия с пользователем
using Tekla.Structures.Datatype;
using TSDT = Tekla.Structures.Datatype;
using System.Globalization;
using Distance = Tekla.Structures.Datatype.Distance;
using Tekla.Structures;
using static Tekla.Structures.Model.Part;
using System.Collections.Generic;
using Tekla.Structures.Drawing;

namespace ContourPoint
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Глобальные параметры

        private Position.PlaneEnum planeEnum;
        private static Model model = new Model();
        private static Model myModel = new Model();
        private static ArrayList ObjectList = new ArrayList();
        private static ArrayList positionComboBox = new ArrayList();
        public TSM.ModelObject myObject { get; private set; }
        int nFaces = 0, nLoops = 0, nVertexes = 0;

        #endregion
        #region Форма WPF
        public MainWindow()
        {
            InitializeComponent();
            positionComboBox.Add("Середина");
            positionComboBox.Add("Слева");
            positionComboBox.Add("Справа");
            OnPlane.ItemsSource = positionComboBox;
            OnPlane.SelectedIndex = 0;
        }
        #endregion
        #region Проверка подключения к TS
        public bool InitializeConnection()
        {
            Model _model = new Model();
            if (_model.GetConnectionStatus()) { model = _model; return true; }
            else { return false; }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!InitializeConnection())
            {
                MessageBox.Show(
                    "Привет!" +
                    "\n" +
                    "\n1. Никита тебе не удалось подключиться к TS" +
                    "\n2. Убедись пожалуйста что программа TS нужной версии" +
                    "\n3. Текущее окно после нажатия ок, закроется");
                this.Close();
            }
        }
        #endregion
        #region Cоздание балки с помощью указания точек для вставки балки

        private void btn_CreateBeam_Click(object sender, RoutedEventArgs e)
        {
            Model myModel = new Model();

            if (myModel.GetConnectionStatus())
            {
                TSMUI.Picker _picker = new TSMUI.Picker();


                TSG.Point startPoint = _picker.PickPoint("укажи начальную точку");
                TSG.Point endPoint = _picker.PickPoint("укажи конечную точку");

                Beam myBeam = new Beam(startPoint, endPoint);

                myBeam.Profile.ProfileString = "I30B1_20_93";
                myBeam.Name = "НИКИТА";
                myBeam.Material.MaterialString = "C245";
                myBeam.Class = "1";

                myBeam.Insert();
                myModel.CommitChanges();
            }

        }
        #endregion
        #region Создание контурной пластины с помощью указания точек
        public void btn_CreateContourPlate_Click(object sender, RoutedEventArgs e)
        {
            Model myModel = new Model();
            //обратился к базовому классу Picker() и создал обращение к UI 
            TSMUI.Picker _picker = new TSMUI.Picker();
            //создал массив и сказал что бы он поместил в массив то что укажу указанным методом который находится в библиотеке TSM
            ArrayList pickedPoints = _picker.PickPoints(TSMUI.Picker.PickPointEnum.PICK_POLYGON, "Укажи точки");
            //Создаю массив для точек которые будут добавлены в массив с помощью foreach
            ArrayList contourPoints = new ArrayList();
            //Создал тип данных (который прописан в базовом классе ТS) и указал на то что это будет !!указано пользователем!! в pickedPoints
            foreach (Point point in pickedPoints)
            {
                //Говорю что бы foreach поместил все точки pickedPoints с указанием что именно я хотел бы создать
                TSM.ContourPoint contourPoint = new TSM.ContourPoint(point, new Chamfer());
                //и добавил в массив с указанием именно точек  
                contourPoints.Add(contourPoint);
            }
            #region Создать статические параметры создаваемой пластины
            ContourPlate CP = new ContourPlate();
            CP.Contour.ContourPoints = contourPoints;
            CP.Name = "НИКИТА";
            CP.Profile.ProfileString = "PL500";
            CP.Material.MaterialString = "C245";
            CP.Class = "1";
            #endregion

            // Создать
            CP.Insert();
            // закоммитить
            myModel.CommitChanges();

        }
        #endregion
        #region работа с компонентом

        public int ConnectionNumber { get; set; }//номера компонента

        /// <summary>
        /// Add - Добавляет объект в конец
        /// AddRange -  Добавляет элементы указанной коллекции в конец списка
        /// Remove - Удаляет первое вхождение указанного объекта из коллекции 
        /// RemoveAt - Удаляет элемент списка с указанным индексом
        /// Contains - Определяет, входит ли элемент в коллекцию
        /// IndexOf - Осуществляет поиск указанного объекта и возвращает отсчитываемый от нуля индекс
        ///           первого вхождения, найденного в пределах всего списка
        /// LastIndexOf - Осуществляет поиск указанного объекта и возвращает отсчитываемый от нуля индекс
        ///               последнего вхождения, найденного в пределах всего списка 
        /// [] - Возвращает или задает элемент по указанному индексу
        /// Insert - Вставляет элемент в коллекцию по указанному индексу
        /// Count - Получает число элементов, содержащихся в коллекции
        /// Clear - Удаляет все элементы из коллекции 
        /// Сортировка списка
        /// Sort или Sort(функция_сравнения)
        /// Reverse
        /// </summary>
        public List<Beam> Beams { get; set; } = new List<Beam>();

        private void btn_CreateComponent_Click(object sender, RoutedEventArgs e)
        {
            #region
            //Model model = new Model();
            //TSMUI.Picker _picker = new TSMUI.Picker();
            //ModelObject modelObject = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "выбери объект дубина");

            //Part cp = modelObject as Part;

            //Beam B1 = new Beam();
            //Beam B2 = new Beam();

            ////B1.CompareTo= cp;//Деталь, которую нужно скрепить болтами
            ////B2.PartToBoltTo = cp;


            //Connection C = new Connection();
            //C.Name = "Проверка вставки компонента!";
            //C.Number = 144;


            //C.LoadAttributesFromFile("standard");
            //C.UpVector = new TSG3D.Vector(0, 0, 0);
            //C.PositionType = PositionTypeEnum.COLLISION_PLANE;

            //C.SetPrimaryObject(B1);
            //C.SetSecondaryObject(B2);

            //C.SetAttribute("e2", 0.0);
            //C.SetAttribute("e1", 0.0);

            //if (!C.Insert())
            //{
            //    Console.WriteLine("Connection Insert failed");
            //}
            //else
            //{
            //    Console.WriteLine(C.Identifier.ID);

            //    //Double DValue = 0.0;

            //    //if (!C.GetAttribute("e2", ref DValue) || DValue != 10)
            //    //    Console.WriteLine("Connection GetAttribute failed");


            //}
            //model.CommitChanges();
            #endregion
            Model model = new Model();
            TSMUI.Picker picker = new TSMUI.Picker();
            var modelObject = picker.PickObjects(TSMUI.Picker.PickObjectsEnum.PICK_N_PARTS);

            TSM.Beam b1 = Beams[0];//start point
            //TSM.Beam b2 = Beams[1];//end point
            Beams.Add(b1);
            #region
            //Beams.Add(b2);
            //B1.CompareTo= cp;//Деталь, которую нужно скрепить болтами
            //B2.PartToBoltTo = cp;
            //while (modelObject.MoveNext())
            //{
            //    /*
            //     * 1. Создаем цикл if (если) на вход которому подаем объект котрый создан выше текущий(Current) var obj 
            //     * запросим этот объект через GetType()
            //     * преравняем через Equals (равно) 
            //     * typeof возвращает строку, указывающую тип операнда. 
            //     * Вывод: Если тип обоъекта в перебираемом цикле равен TSM.Beam тогда...
            //     */
            //    if (modelObject.Current.GetType().Equals(typeof(Beam)))
            //    {
            //        //Создаем новую переменную =>>> b и приводим к (obj.Current) TSM.Beam
            //        Beam b = modelObject.Current as Beam;
            //        /*
            //         * внутри этого цикла создадим еще один цикл который проверяет является ли тип(Type) выбраной балки
            //         * колонна то b приводится к контейнеру column если нет(else) тогда beam
            //         */                    
            //    }
            //    /*
            //     * в цикле ниже я проверяю те элементы типы которых не сответствуют классу TSM.Beam
            //     */                
            //}
            #endregion
            TSM.Connection C = new TSM.Connection();
            C.Name = "Проверка вставки компонента!";
            C.Number = 144;

            C.LoadAttributesFromFile("standard");
            C.UpVector = new TSG3D.Vector(0, 0, 0);
            C.PositionType = PositionTypeEnum.COLLISION_PLANE;
            C.SetSecondaryObject(b1);
            //C.Insert();//создать компонента с лева

            //C.SetPrimaryObject(b2);
            //C.Insert();//создать компонента с права
            C.SetAttribute("e2", 0.0);
            C.SetAttribute("e1", 0.0);

            for (int c = 2; c < Beams.Count; c++)
            {
                TSM.Beam n = Beams[c];
                C.SetSecondaryObject(n);
                C.SetSecondaryObject(b1);
                C.Insert();//создать компонента с лева

                //C.SetPrimaryObject(b2);
                //C.Insert();//создать компонента с права
            }
            model.CommitChanges();
            #region
            //Model myModel = new Model();
            //Connection Database = new Connection();


            //    var handler = myModel.GetWorkPlaneHandler();
            //    var modelcoordinate = handler.GetCurrentTransformationPlane();

            //    foreach (var item in Database.OfType())
            //    {
            //        var coordinata = item.GetCoordinateSystem();
            //        TransformationPlane plane = new TransformationPlane(coordinata);

            //        handler.SetCurrentTransformationPlane(plane);
            //        item.Select();

            //        var sp = item.StartPoint;
            //        var ep = item.EndPoint;

            //        double width = 0;
            //        double height = 0;

            //        item.GetReportProperty("WIDTH", ref width);
            //        item.GetReportProperty("HEIGHT", ref height);


            //        Point start = new Point(sp.X, sp.Y, sp.Z + width * 0.5);
            //        Point end = new Point(ep.X, ep.Y, ep.Z + width * 0.5);

            //        CustomPart co = new CustomPart(start, end);
            //        co.Number = -1; //Use −1 for custom parts
            //        co.Name = "__Handrail";
            //        co.Position.Plane = Position.PlaneEnum.MIDDLE;
            //        co.Position.Rotation = Position.RotationEnum.FRONT;
            //        co.Position.Depth = Position.DepthEnum.MIDDLE;


            //        co.SetAttribute("post_end", 100);
            //        co.SetAttribute("adjacent_structures", 0);
            //        co.Insert();
            //        //Removing beam after replacing:
            //        //item.Delete();
            //    }
            //    //Updating the model view:
            //    handler.SetCurrentTransformationPlane(modelcoordinate);

            //    myModel.CommitChanges();

            //    //Passing nothing to data grid:
            //    return;
            #endregion
        }
        #endregion
        #region Создание балки с заданными параметрами
        public int Category { get; set; }
        public string ProjectImya { get; set; }
        public string GostName { get; set; }
        public double SpravMassa { get; set; }
        public double weigth { get; set; }
        public object Database { get; private set; }
        public int PhaseNumber { get; set; }

        private void btn_Create_New1_Click(object sender, RoutedEventArgs e)
        {
            Model myModel = new Model();
            //int phaseUser = model.GetInfo().CurrentPhase;//создать объект по текущей стадии
            int phaseUser = 6;//создать объект по нужной стадии            
            var phase_collection = myModel.GetPhases();
            Phase phase = null;
            var phase_enum = phase_collection.GetEnumerator();
            while (phase_enum.MoveNext())
            {
                var current = phase_enum.Current as Phase;
                if (current.PhaseNumber == phaseUser)
                {
                    phase = current;
                }                
                
            }            
            Picker picker = new Picker();
            Point startPoint = picker.PickPoint("укажи начальную точку");
            Point endPoint = picker.PickPoint("укажи конечную точку");
            Beam myBeam = new Beam(startPoint, endPoint);
            myBeam.Profile.ProfileString = cb_Profile.Text;
            myBeam.Name = cb_Name.Text;
            myBeam.Material.MaterialString = cb_Material.Text;
            myBeam.Class = cb_Class.Text;
            myBeam.PartNumber.Prefix = PosT.Text;
            myBeam.AssemblyNumber.Prefix = PosN.Text;
            myBeam.PartNumber.StartNumber = Convert.ToInt16(AssamT.Text);
            myBeam.AssemblyNumber.StartNumber = Convert.ToInt16(AssamN.Text);
            myBeam.Position.Plane = UpdatePlan(OnPlane.SelectedIndex);
            myBeam.Position.Depth = Position.DepthEnum.FRONT;
            myBeam.Position.Rotation = Position.RotationEnum.FRONT;
            myBeam.Finish = "НИКИТА";
            myBeam.DeformingData = new DeformingData();
            myBeam.CastUnitType = CastUnitTypeEnum.CAST_IN_PLACE;//тип изготовления заводской или по месту
            myBeam.CastUnitType.CompareTo(Tag);
            //ru_nesush_konstr
            myBeam.Insert();
            myBeam.SetPhase(phase);      
            myBeam.Modify();
            Category = 0;//Категория 0=5 , 1=6 ... 13=18
            myBeam.SetUserProperty("cm_kat", Category);
            myBeam.Modify();
            ProjectImya = "НИКИТА2";
            myBeam.SetUserProperty("ru_proektnoe_imya", ProjectImya);
            myBeam.Modify();
            GostName = "НИКИТА3";
            myBeam.SetUserProperty("ru_gost_name", GostName);
            myBeam.Modify();
            SpravMassa = 3.14;
            myBeam.SetUserProperty("ru_sprav_massa", SpravMassa);
            myBeam.Modify();
            double weigth = 0;
            myBeam.GetReportProperty("WEIGTH", ref weigth);
            myBeam.Modify();
            string nesushKonstr = "Да";
            myBeam.SetUserProperty("ru_nesush_konstr", nesushKonstr);
            myBeam.Modify();
            myModel.CommitChanges();
        }
        private Position.PlaneEnum UpdatePlan(int OnPlane)
        {
            switch (OnPlane)
            {
                case 0:
                    return planeEnum = Position.PlaneEnum.MIDDLE;
                case 1:
                    return planeEnum = Position.PlaneEnum.LEFT;
                case 2:
                    return planeEnum = Position.PlaneEnum.RIGHT;

                default:
                    return planeEnum = Position.PlaneEnum.MIDDLE;
            }
        }
        #endregion
        #region Создание болтовой группы
        private void btn_CreateBoltStandart_Click(object sender, RoutedEventArgs e)
        {
            #region 1. Выбор в модели

            Model model = new Model();

            TSMUI.Picker _picker = new TSMUI.Picker();
            //выбор детали с помощью Picker()
            TSM.ModelObject mo = _picker.PickObject(TSMUI.Picker.PickObjectEnum.PICK_ONE_PART);

            #endregion

            #region 2. Преобразует тип полученной детали в указанный тип
            //говорю что, то что я выбрал, его тип  -деталь! Привел к типу Part(условие прописанно в базовом классе)
            TSM.Part cp = mo as TSM.Part;

            #endregion

            #region 3. Массив для болтов

            TSM.BoltArray B = new TSM.BoltArray();

            #endregion

            #region 4. Выбор которой и к которой необходимо прикрепится болтами

            B.PartToBeBolted = cp;//Деталь, которую нужно скрепить болтами
            B.PartToBoltTo = cp;//Деталь, к которой нужно прикрутить

            #endregion

            #region  5. Точка болтового массива

            B.FirstPosition = new Point(3000, 6000, 0);
            B.SecondPosition = new Point(3000, 60000, 0);

            #endregion

            #region 6. Параметры болтов

            B.BoltSize = Convert.ToDouble(cb_boltSize.Text);
            B.Tolerance = 3;
            B.BoltStandard = cb_gostBolt.Text;
            B.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
            B.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;//THREAD_IN_MATERIAL_NO- без болта

            #endregion

            #region 7. Положение болта в модели

            B.Position.Depth = Position.DepthEnum.MIDDLE;
            B.Position.Plane = Position.PlaneEnum.MIDDLE;
            B.Position.Rotation = Position.RotationEnum.FRONT;

            #endregion

            #region Интервал по X

            B.AddBoltDistX(100);
            B.AddBoltDistX(90);
            B.AddBoltDistX(80);

            #endregion

            #region Интервал по Y

            B.AddBoltDistY(70);
            B.AddBoltDistY(60);
            B.AddBoltDistY(50);

            #endregion

            #region Смещение по X

            B.StartPointOffset.Dx = 100;

            #endregion

            #region Смещение по Y

            B.StartPointOffset.Dy = 200;

            #endregion

            #region Условие при котором создаются болты
            if (!B.Insert())
            {
                MessageBox.Show("ss!");
            }
            #endregion

            #region Внести изменения в модель
            model.CommitChanges();
            #endregion
        }
        #endregion
        #region Создание обрезки инструментом подгонка
        private void btn_createFitting_Click(object sender, RoutedEventArgs e)
        {
            Model myModel = new Model();

            TSM.Beam beam = new TSM.Beam();
            beam.StartPoint = new Point(0, 0, 0);
            beam.EndPoint = new Point(1000, 0, 0);
            beam.Profile.ProfileString = "PP150X100X7.0_32931_2015";
            beam.Material.MaterialString = "C245";
            beam.Class = "1";

            beam.Insert();

            Fitting Fitting = new Fitting();

            Fitting.Plane = new Plane();
            Fitting.Plane.Origin = new Point(500, 0, 0);
            Fitting.Plane.AxisX = new TSG.Vector(0, 1000, 0);
            Fitting.Plane.AxisY = new TSG.Vector(0, 0, -1000);

            Fitting.Father = beam;
            Fitting.Insert();
            myModel.CommitChanges();
        }

        #endregion
        #region Создание обрезки инструментом срез(по линии)
        private void btn_createCut_Click(object sender, RoutedEventArgs e)
        {
            Model myModel = new Model();

            Beam beam = new Beam();
            beam.StartPoint = new Point(0, 0, 0);
            beam.EndPoint = new Point(5000, 0, 0);
            beam.Profile.ProfileString = "PP150X100X7.0_32931_2015";
            beam.Material.MaterialString = "C245";
            beam.Class = "2";
            beam.Insert();

            CutPlane cutPlane = new CutPlane();
            cutPlane.Plane = new Plane();
            cutPlane.Plane.Origin = new Point(1000, 0, 0);
            cutPlane.Plane.AxisX = new TSG.Vector(0, 100, 0);
            cutPlane.Plane.AxisY = new TSG.Vector(0, 0, -1000);
            cutPlane.Father = beam;
            cutPlane.Insert();

            myModel.CommitChanges();

        }

        #endregion
        #region Создать сварку

        private void btn_createWeld_Click(object sender, RoutedEventArgs e)
        {
            TSMUI.Picker picker = new TSMUI.Picker();
            TSM.ModelObject PrimaryPart = picker.PickObject(TSMUI.Picker.PickObjectEnum.PICK_ONE_PART, "ВЫБЕРИ МЕНЯ, ЭХХХ, ВЫБЕРИ МЕНЯ!");
            TSM.ModelObject SecondaryPart = picker.PickObject(TSMUI.Picker.PickObjectEnum.PICK_ONE_PART, "И ВЫБЕРИ МЕНЯ!");
            ObjectList.Add(PrimaryPart);
            ObjectList.Add(SecondaryPart);

            TSM.Weld weld = new TSM.Weld();
            weld.MainObject = PrimaryPart;
            weld.SecondaryObject = SecondaryPart;//второстепенная деталь
            weld.TypeAbove = BaseWeld.WeldTypeEnum.STEEP_FLANKED_BEVEL_GROOVE_SINGLE_V_BUTT;//тип сварного шва 1
            weld.TypeBelow = BaseWeld.WeldTypeEnum.STEEP_FLANKED_BEVEL_GROOVE_SINGLE_V_BUTT;//тип сварного шва 2
            weld.SizeAbove = 6;//катет 1
            weld.SizeBelow = 6;//катет 2
            weld.IncrementAmountAbove = 1;//номер приращения
            weld.Position = TSM.Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;//+x
            weld.FinishAbove = TSM.Weld.WeldFinishEnum.WELD_FINISH_CHIP;
            weld.FinishBelow = TSM.Weld.WeldFinishEnum.WELD_FINISH_CHIP;
            weld.PrefixAboveLine = "Префикс над линией";
            weld.PrefixBelowLine = "Префикс под линией";
            weld.AroundWeld = true;
            weld.IntermittentType = BaseWeld.WeldIntermittentTypeEnum.STAGGERED_INTERMITTENT;//форма шва
            weld.ShopWeld = false;// заводской или нет
            weld.ContourAbove = BaseWeld.WeldContourEnum.WELD_CONTOUR_CONVEX;
            weld.ContourBelow = BaseWeld.WeldContourEnum.WELD_CONTOUR_CONVEX;
            weld.Preparation = BaseWeld.WeldPreparationTypeEnum.PREPARATION_MAIN;//Подготовка:



            if (!weld.Insert())
            {
                MessageBox.Show("Да, все норм!");
            }
            model.CommitChanges();
        }


        #endregion
        #region Создание точки
        private void btn_createPoint_Click(object sender, RoutedEventArgs e)
        {
            Point point = new Point(1000, 1000, 1000);
            ControlPoint controlPoint = new ControlPoint(point);
            bool Result = false;
            Result = controlPoint.Insert();
            model.CommitChanges();
        }


        #endregion
        #region Перебирать твердое тело. Считает сколько в каждой детали лицевых сторон, циклов, вершин
        /// <summary>
        /// Пример твердотельного теста объясняет, 
        /// как перебирать твердое тело «Балки» 
        /// и перечислять его основное содержимое,
        /// такое как грани, петли и вершины.
        /// </summary>
        /// <returns></returns>
        private Beam insertBeam()
        {
            TSG3D.Point startPoint = new TSG3D.Point(5000, 0, 0);
            TSG3D.Point endPoint = new TSG3D.Point(0, 0, 0);
            Beam beam = new Beam(endPoint, startPoint);
            beam.Profile.ProfileString = "I20B2_57837_2017";
            beam.Material.MaterialString = "C245";
            beam.Name = "Никита";
            beam.Finish = "Normal";
            beam.Class = "3";
            if (!beam.Insert())
            {
                MessageBox.Show("Балка не создалась!");
            }
            else
            {
                ObjectList.Add(beam);
            }
            model.CommitChanges();
            return beam;
        }
        private void btn_CreateBeamSolid_Click(object sender, RoutedEventArgs e)
        {
            Beam beam = insertBeam();
            if (!beam.Select())
            {
                MessageBox.Show("Пипец!");
            }
            Solid Solid = beam.GetSolid();
            FaceEnumerator faceEnumerator = Solid.GetFaceEnumerator();

            //Циклы считают лицевые стороны(Face),циклы(Loop) и вершины(Vertexes) всех объектов в модели

            while (faceEnumerator.MoveNext()) //используется для перечисления граней твердого тела
            {
                nFaces++;
                Face Face = faceEnumerator.Current as Face;
                LoopEnumerator loopEnum = Face.GetLoopEnumerator();
                while (loopEnum.MoveNext()) //используется для перечисления циклов твердого тела
                {
                    nLoops++;
                    Loop loop = loopEnum.Current as Loop;
                    VertexEnumerator vertexEnum = loop.GetVertexEnumerator();

                    while (vertexEnum.MoveNext())//используется для перечисления вершин твердого тела
                    {
                        nVertexes++;
                    }
                }
            }


            MessageBox.Show(" количествоЛицевыхСторон = " + nFaces + " количествоЦиклов = " + nLoops + " количествоВершин = " + nVertexes);
            MessageBox.Show("Тест завершен!");
        }


        #endregion
        #region Создать точку с помощью UI
        private void btn_createPoint2_Click(object sender, RoutedEventArgs e)
        {
            TSMUI.Picker picker = new TSMUI.Picker();
            Point point = picker.PickPoint("укажи точку");
            ControlPoint controlPoint = new ControlPoint(point);
            bool Result = false;
            Result = controlPoint.Insert();
            model.CommitChanges();
        }


        #endregion
        #region Отсканируйте всю модель найти все балки и изменить их класс на указанный ниже!
        private void btn_CreateFindClassObject_Click(object sender, RoutedEventArgs e)
        {
            TSMUI.ModelObjectSelector selector = new TSMUI.ModelObjectSelector();
            ModelObjectEnumerator modelObjectEnumerator = selector.GetSelectedObjects();

            foreach (TSM.ModelObject obj in modelObjectEnumerator)
            {
                Beam beam = obj as Beam;
                if (beam != null)
                {
                    beam.Class = "7";
                    beam.Modify();
                }
            }
            model.CommitChanges();

        }


        #endregion
        #region Отсканируйте всю модель, чтобы получить только контурные пластины, и измените свойства.

        private void btn_CreateFindClassObject2_Click(object sender, RoutedEventArgs e)
        {
            TSMUI.ModelObjectSelector selector = new TSMUI.ModelObjectSelector();
            ModelObjectEnumerator modelObjectEnumerator = selector.GetSelectedObjects();



            foreach (TSM.ModelObject obj in modelObjectEnumerator)
            {
                ContourPlate contourPlate = obj as ContourPlate;
                if (contourPlate != null)
                {
                    contourPlate.Profile.ProfileString = "PL200";
                    contourPlate.Class = "2";
                    contourPlate.Material.MaterialString = "C355";
                    contourPlate.Finish = "sdfdsf";
                    contourPlate.Modify();
                }
                else
                {
                    MessageBox.Show("промах!");
                }
            }
            model.CommitChanges();

        }




        #endregion
        #region Приложить точечную нагрузку
        private void btn_CreateFindClassObject3_Click(object sender, RoutedEventArgs e)
        {

            Beam FatherBeam = new Beam(new Point(3000, 6000, 0), new Point(4000, 6000, 0));
            FatherBeam.Profile.ProfileString = "I20B2_57837_2017";
            FatherBeam.Material.MaterialString = "C245";
            if (!FatherBeam.Insert())
                Console.WriteLine("Father Beam Insert failed!");

            LoadPoint L = new LoadPoint();
            L.P = new TSG.Vector(3000, 4000, 5000);
            L.Moment = new TSG.Vector(6000, 7000, 8000);
            L.Position = new Point(3000, 6000, 0);

            L.FatherId = FatherBeam.Identifier;

            L.AutomaticPrimaryAxisWeight = true;
            //L.BoundingBoxDx = 500;
            //L.BoundingBoxDy = 500;
            L.BoundingBoxDz = 500;
            L.LoadDispersionAngle = 1;
            L.PartFilter = "testing";
            L.PartNames = Load.LoadPartNamesEnum.LOAD_PART_NAMES_INCLUDE;
            L.PrimaryAxisDirection = new TSG.Vector(1000, 500, 0);
            L.Spanning = Load.LoadSpanningEnum.LOAD_SPANNING_SINGLE;
            L.Weight = 2;
            L.CreateFixedSupportConditionsAutomatically = true;

            if (!L.Insert())
                Console.WriteLine("LoadPoint Insert failed!");
            FatherBeam.Insert();
            model.CommitChanges();
        }


        #endregion
        #region риложить нагрузку кв. м2
        private void btn_CreateFindClassObject4_Click(object sender, RoutedEventArgs e)
        {
            Model model = new Model();


            LoadArea L = new LoadArea();
            L.P1 = new TSG.Vector(1000, 2000, 3000);
            L.P2 = new TSG.Vector(4000, 5000, 6000);
            L.P3 = new TSG.Vector(7000, 8000, 9000);
            L.DistanceA = 5;
            L.Position1 = new Point(6000, 6000, 0);
            L.Position2 = new Point(8000, 6000, 0);
            L.Position3 = new Point(8000, 9000, 0);
            L.LoadForm = LoadArea.AreaLoadFormEnum.LOAD_FORM_AREA_PARALLELOGRAM;

            L.AutomaticPrimaryAxisWeight = true;
            L.BoundingBoxDx = 500;
            L.BoundingBoxDy = 500;
            L.BoundingBoxDz = 500;
            L.LoadDispersionAngle = 5;
            L.PartFilter = "testing";
            L.PartNames = Load.LoadPartNamesEnum.LOAD_PART_NAMES_INCLUDE;
            L.PrimaryAxisDirection = new TSG.Vector(1000, 500, 0);
            L.Spanning = Load.LoadSpanningEnum.LOAD_SPANNING_SINGLE;
            L.Weight = 2;
            L.CreateFixedSupportConditionsAutomatically = true;

            if (!L.Insert())
                Console.WriteLine("LoadArea Insert failed!");
            model.CommitChanges();
        }


        #endregion
        #region Приложить погонную нагрузку
        private void btn_CreateFindClassObject5_Click(object sender, RoutedEventArgs e)
        {
            Model model = new Model();
            LoadLine L = new LoadLine();
            L.P1 = new TSG.Vector(0000, 0000, -1000);
            L.P2 = new TSG.Vector(0000, 0000, -1000);
            L.DistanceA = 10;
            L.DistanceB = 6;
            L.Torsion1 = 1000;
            L.Torsion2 = 2000;
            L.Position1 = new Point(3000, 12000, 0);
            L.Position2 = new Point(4000, 12000, 0);
            L.LoadForm = LoadLine.LineLoadFormEnum.LOAD_FORM_LINE_1;

            L.AutomaticPrimaryAxisWeight = true;
            L.BoundingBoxDx = 500;
            L.BoundingBoxDy = 500;
            L.BoundingBoxDz = 500;
            L.LoadDispersionAngle = 5;
            L.PartFilter = "testing";
            L.PartNames = Load.LoadPartNamesEnum.LOAD_PART_NAMES_INCLUDE;
            L.PrimaryAxisDirection = new TSG.Vector(1000, 500, 0);
            L.Spanning = Load.LoadSpanningEnum.LOAD_SPANNING_SINGLE;
            L.Weight = 2;
            L.CreateFixedSupportConditionsAutomatically = true;

            if (!L.Insert())
                Console.WriteLine("LoadLine Insert failed!");

            model.CommitChanges();
        }


        #endregion
        #region UDA
        //User Defined Attribute(UDA) -Определенный пользователем атрибут
        private void btn_CreateFindClassObject6_Click(object sender, RoutedEventArgs e)
        {
            Model model = new Model();
            int myValue = 3;

            TSMUI.Picker _picker = new TSMUI.Picker();//Подключился к юзер интерфейсу
            TSM.Part part = _picker.PickObject(TSMUI.Picker.PickObjectEnum.PICK_ONE_PART, "Выбери объект дубина!") as TSM.Part;//условие выбора одного объекта и если не выбрал тогда появляется сообщение 

            if (part != null)//проверка существует ли деталь
            {
                //если существует деталь тогда, создаю свое имя "Хер_знает_что_творится! и присвой ему value =>> "Значение" в данном случае string
                part.SetUserProperty("Хер_знает_что_творится! ", 3);
            }
            //получи значение для myValue через "Хер_знает_что_творится!: "
            part.GetUserProperty("Хер_знает_что_творится!: ", ref myValue);
            //передаю это значение в MessageBox
            MessageBox.Show("Определенный пользователем атрибут: " + myValue);
            model.CommitChanges();
        }


        #endregion
        #region Работа с UDA - user coordinate system(пользовательская система координат)
        private void btn_CreateFindClassObject7_Click(object sender, RoutedEventArgs e)
        {
            #region 1. Подключился к модели
            Model model = new Model();
            #endregion
            #region 2. Подключился к пользовательскому итерфейсу
            TSMUI.Picker picker = new TSMUI.Picker();
            #endregion
            #region 3. Обеспечил выбор в модели одного объекта PICK_ONE_OBJECT

            TSM.ModelObject modelObject = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "выбери объект дубина");

            #endregion
            #region 4. Подключился  к текущей системе координат и создал контейнер для координат получаемого у объекта

            CoordinateSystem partCoordinate = modelObject.GetCoordinateSystem();

            #endregion
            #region 5. Трансформировал план под координаты выбранного объекта

            TransformationPlane partPlane = new TransformationPlane(partCoordinate);

            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(partPlane);
            #endregion
            #region 6. Внесем изменения в модель(закомитим!)
            model.CommitChanges();
            #endregion
        }


        #endregion
        #region Установка UDA в нужное положение 
        private void btn_CreateFindClassObject8_Click(object sender, RoutedEventArgs e)
        {
            #region 1. Подключился к модели
            Model model = new Model();
            #endregion
            #region 2. Подключился  к текущей системе координат и установить новую локальную систему координат
            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
            #endregion
            #region 3. Создание точки с координатами 0,0,0 и единичных векторов по направлению X и Y
            Point Orgin = new Point(0, 0, 0);
            TSG3D.Vector X = new TSG3D.Vector(-1, 0, 0);
            TSG3D.Vector Z = new TSG3D.Vector(0, 0, -1);
            #endregion
            #region 4. Передаю точку и созданные единичные координаты векторов для изменения положения UCS
            TransformationPlane XZ_Plane = new TransformationPlane(Orgin, X, Z);
            #endregion
            #region 5. Внести изменения в UCS
            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(XZ_Plane);
            #endregion
            #region 6. Внесем изменения в модель(закомитим!)
            model.CommitChanges();
            #endregion
        }
        #endregion
        #region Создать и выделить(вбрать нужные объекты) в модели
        private void btn_CreateFindClassObject9_Click(object sender, RoutedEventArgs e)
        {
            Model model = new Model();
            Beam B = new Beam(new Point(0, 0, 0), new Point(0, 0, 6000));
            Beam B1 = new Beam(new Point(0, 1000, 0), new Point(0, 1000, 6000));
            Beam B2 = new Beam(new Point(0, 2000, 0), new Point(0, 2000, 6000));

            #region параметры балки
            B.Profile.ProfileString = "I30B1_20_93";
            B.Name = "НИКИТА";
            B.Material.MaterialString = "C245";
            B.Class = "1";
            B1.Profile.ProfileString = "I30B1_20_93";
            B1.Name = "НИКИТА";
            B1.Material.MaterialString = "C245";
            B1.Class = "2";
            B2.Profile.ProfileString = "I30B1_20_93";
            B2.Name = "НИКИТА";
            B2.Material.MaterialString = "C245";
            B2.Class = "3";
            #endregion

            B.Insert();
            B1.Insert();
            B2.Insert();

            ArrayList ObjectsToSelect = new ArrayList();
            ObjectsToSelect.Add(B);
            ObjectsToSelect.Add(B1);
            ObjectsToSelect.Add(B2);
            TSMUI.ModelObjectSelector MS = new TSMUI.ModelObjectSelector();

            MS.Select(ObjectsToSelect);
            model.CommitChanges();
        }
        #endregion
        #region Создать временную графику в активном окне модели
        private void btn_CreateFindClassObject10_Click(object sender, RoutedEventArgs e)
        {
            GraphicsDrawer drawer = new GraphicsDrawer();

            drawer.DrawText(new Point(0.0, 1000.0, 1000.0), "Щеголев Никита Николаевич", new Color(1.0, 0.5, 0.0));
            drawer.DrawLineSegment(new Point(0.0, 0.0, 0.0), new Point(1000.0, 1000.0, 1000.0), new Color(1.0, 0.0, 0.0));

            Mesh mesh = new Mesh();
            mesh.AddPoint(new Point(0.0, 0.0, 0.0));
            mesh.AddPoint(new Point(3000.0, 0.0, 0.0));
            mesh.AddPoint(new Point(4000.0, 5000.0, 0.0));
            mesh.AddPoint(new Point(0.0, 5000.0, 0.0));
            mesh.AddTriangle(0, 1, 2);
            mesh.AddTriangle(0, 2, 3);
            mesh.AddLine(0, 1);
            mesh.AddLine(1, 2);
            mesh.AddLine(2, 3);
            mesh.AddLine(3, 1);

            drawer.DrawMeshSurface(mesh, new Color(1.0, 0.0, 0.0, 0.5));
            drawer.DrawMeshLines(mesh, new Color(0.0, 0.0, 1.0));
        }
        #endregion
        #region Выбрать грань объекта и вывести данные 
        private void btn_CreateFindClassObject11_Click(object sender, RoutedEventArgs e)
        {

            Picker picker = new Picker();

            try
            {
                PickInput input = picker.PickFace("Выбери плоскость объект!");
                IEnumerator myEnum = input.GetEnumerator();
                while (myEnum.MoveNext())
                {
                    InputItem item = myEnum.Current as InputItem;

                    if (item.GetInputType() == InputItem.InputTypeEnum.INPUT_N_OBJECTS)
                    {
                        TSM.ModelObject modelObject = item.GetData() as TSM.ModelObject;
                        MessageBox.Show(modelObject.ToString());
                    }
                    if (item.GetInputType() == InputItem.InputTypeEnum.INPUT_POLYGON)
                    {
                        ArrayList points = item.GetData() as ArrayList;
                        MessageBox.Show((points[0] as Point).ToString());
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }


        }
        #endregion
        private void btn_CreateFindClassObject12_Click(object sender, RoutedEventArgs e)
        {
            Model model = new Model();


            Point Point1 = new Point(4000, 4000, 0);
            Point Point2 = new Point(7000, 4000, 0);
            Point Point3 = new Point(4000, 6000, 0);
            Point Point4 = new Point(7000, 6000, 0);
            Beam Beam1 = new Beam(Point1, Point2);
            Beam Beam2 = new Beam(Point3, Point4);
            Beam1.Profile.ProfileString = "HEA400";
            Beam1.Finish = "PAINT";
            Beam2.Profile.ProfileString = "HEA400";
            Beam2.Finish = "PAINT";
            Beam1.Insert();
            Beam2.Insert();
            Phase Beam1Phase = new Phase();
            Beam1.GetPhase(out Beam1Phase);
            int PhaseNumber = Beam1Phase.PhaseNumber;
            string PhaseName = Beam1Phase.PhaseName;
            string PhaseComment = Beam1Phase.PhaseComment;
            Phase Beam2Phase = new Phase();
            Beam2.GetPhase(out Beam2Phase);
            if (Beam1Phase.PhaseNumber != Beam2Phase.PhaseNumber)
            {
                if (Convert.ToBoolean(Beam1Phase.IsCurrentPhase))
                    Beam2.SetPhase(Beam1Phase);
                else
                    Beam1.SetPhase(Beam2Phase);
            }

            Beam1Phase.PhaseName = "fsdfkasdkjfkjdas";
            Beam1Phase.PhaseNumber = 5;
            Beam1Phase.PhaseComment = "fsdfkasdkjfkj";
            Beam1Phase.Modify();
            model.CommitChanges();
        }
    }
}
