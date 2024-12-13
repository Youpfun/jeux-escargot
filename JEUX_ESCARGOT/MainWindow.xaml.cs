using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Drawing;

namespace JEUX_ESCARGOT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int PAS_ESCARGOT = 20;
        private static BitmapImage escargotGauche;
        private static BitmapImage escargotDroit;
        private static DispatcherTimer minuterie;
        private DispatcherTimer saladeTimer;
        private bool saladeEnAttente = false;
        private static Random rnd = new Random();
        private static Random rndSalade = new Random();
        private static bool gauche, droite, haut, bas;
        private static SoundPlayer sonDeFond;
        private static readonly int VITESSE_SALADE = 5;
        int score = 0, barreDeVie = 3;
        System.Windows.Media.ImageSource[] tabSalades = [new BitmapImage(new Uri("pack://application:,,,/ressource/img/salade1bg.png")), new BitmapImage(new Uri("pack://application:,,,/ressource/img/salade2bg.png")), new BitmapImage(new Uri("pack://application:,,,/ressource/img/salade3bg.png")), new BitmapImage(new Uri("pack://application:,,,/ressource/img/salade4bg.png")), new BitmapImage(new Uri("pack://application:,,,/ressource/img/salade5bg.png")), new BitmapImage(new Uri("pack://application:,,,/ressource/img/salade6bg.png"))];
        public MainWindow()
        {
            InitializeComponent();
            InitBitmaps();
            InitTimer();
            SonDeFond();
            InitSaladeTimer();
        }
        private void InitSaladeTimer()
        {
            saladeTimer = new DispatcherTimer();
            saladeTimer.Interval = TimeSpan.FromSeconds(5);
            saladeTimer.Tick += ReprendreDescente;
        }

        private void ReprendreDescente(object? sender, EventArgs e)
        {
            // Arrêter le timer
            saladeTimer.Stop();

            // Reprendre la descente
            saladeEnAttente = false;
        }

        private void InitBitmaps()
        {
            escargotDroit = new BitmapImage(new Uri("pack://application:,,,/ressource/img/escargot-droit.png"));
            escargotGauche = new BitmapImage(new Uri("pack://application:,,,/ressource/img/escargot-gauche.png"));
            escargot.Source = escargotDroit;
        }
        private void InitTimer()
        {
            minuterie = new DispatcherTimer();
            minuterie.Interval = TimeSpan.FromMilliseconds(9);
            minuterie.Tick += Jeu;
            minuterie.Start();
        }
        private void SonDeFond()
        {
            sonDeFond = new SoundPlayer(Application.GetResourceStream(
            new Uri("pack://application:,,,/JEUX_ESCARGOT;component/ressource/son/test.wav")).Stream);

            // Lire le fichier en arrière-plan (asynchrone)
            sonDeFond.PlayLooping();  // Pour lire en boucle
                                      // player.Play();      // Pour lire une seule fois
        }

        //private void initCadeau()
        //{
        //    Canvas.SetTop(cadeau, 0);
        //    Canvas.SetLeft(cadeau, rnd.Next(0, (int)this.ActualWidth));
        //}
        private void Fenetre_ToucheLevee(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                droite = false;
            }
            else if (e.Key == Key.Left)
            {
                gauche = false;
            }
            else if (e.Key == Key.Up)
            {
                haut = false;
            }
            else if (e.Key == Key.Down)
            {
                bas = false;
            }
        }
        private void Fenetre_TouchePressee(object sender, KeyEventArgs e)
        {
#if DEBUG
            Console.WriteLine(e.Key);
#endif
            if (e.Key == Key.Right)
            {
                droite = true;
            }
            else if (e.Key == Key.Left)
            {
                gauche = true;
            }
            else if (e.Key == Key.Up)
            {
                haut = true;
            }
            else if (e.Key == Key.Down)
            {
                bas = true;
            }
            else if (e.Key == Key.Space)
            {
                if (minuterie.IsEnabled)
                    minuterie.Stop();
                else
                    minuterie.Start();
            }
            else if (e.Key == Key.Escape)
            {
                BasculerMenuPause();
            }
        }
        private void Jeu(object? sender, EventArgs e)
        {
            Random alea = new Random();
            Canvas.SetTop(imageSaladeGauche, Canvas.GetTop(imageSaladeGauche) + VITESSE_SALADE);

            var saladeRect = new System.Drawing.Rectangle(
            (int)Canvas.GetLeft(imageSaladeGauche),
            (int)Canvas.GetTop(imageSaladeGauche),
            (int)imageSaladeGauche.ActualWidth,
            (int)imageSaladeGauche.ActualHeight);

            var voitureRect = new System.Drawing.Rectangle(
            (int)Canvas.GetLeft(voiture),
            (int)Canvas.GetTop(voiture),
            (int)voiture.ActualWidth,
            (int)voiture.ActualHeight);

            var escargotRect = new System.Drawing.Rectangle(
            (int)Canvas.GetLeft(escargot),
            (int)Canvas.GetTop(escargot),
            (int)escargot.ActualWidth,
            (int)escargot.ActualHeight);

            var saladeGaucheRect = new System.Drawing.Rectangle(
            (int)Canvas.GetLeft(imageSaladeGauche),
            (int)Canvas.GetTop(imageSaladeGauche),
            (int)imageSaladeGauche.ActualWidth,
            (int)imageSaladeGauche.ActualHeight);

            if (voitureRect.IntersectsWith(escargotRect))
            {
                barreDeVie--;
                Canvas.SetLeft(escargot, 0);
            }

            if (saladeGaucheRect.IntersectsWith(escargotRect))
            {
                score++;
                Canvas.SetTop(imageSaladeGauche, 0 - imageSaladeGauche.ActualHeight);
            }

            if (gauche)
            {
                escargot.Source = escargotGauche;
                Canvas.SetLeft(escargot, Canvas.GetLeft(escargot) - PAS_ESCARGOT);
                if (Canvas.GetLeft(escargot) < 0)
                    Canvas.SetLeft(escargot, 0);
            }
            if (droite)
            {
                escargot.Source = escargotDroit;
                Canvas.SetLeft(escargot, Canvas.GetLeft(escargot) + PAS_ESCARGOT);
                if (Canvas.GetLeft(escargot) > this.ActualWidth - escargot.Width)
                    Canvas.SetLeft(escargot, this.ActualWidth - escargot.Width);
            }
            if (bas)
            {
                Canvas.SetTop(escargot, Canvas.GetTop(escargot) + PAS_ESCARGOT);
                if (Canvas.GetTop(escargot) > this.ActualHeight - escargot.Height)
                    Canvas.SetTop(escargot, this.ActualHeight - escargot.Height);
            }
            if (haut)
            {
                Canvas.SetTop(escargot, Canvas.GetTop(escargot) - PAS_ESCARGOT);
                if (Canvas.GetTop(escargot) < 0)
                    Canvas.SetTop(escargot, 0);
            }
            if (!saladeEnAttente)
            {
                Canvas.SetTop(imageSaladeGauche, Canvas.GetTop(imageSaladeGauche) + VITESSE_SALADE);

                // Vérifier si la salade est sortie de l'écran
                if (Canvas.GetTop(imageSaladeGauche) > this.ActualHeight)
                {
                    // Mettre en pause la descente
                    saladeEnAttente = true;

                    // Positionner la salade au-dessus de l'écran
                    Canvas.SetTop(imageSaladeGauche, -imageSaladeGauche.ActualHeight);
                    Canvas.SetLeft(imageSaladeGauche, rnd.Next(0, (int)this.ActualWidth - (int)imageSaladeGauche.ActualWidth));
                    imageSaladeGauche.Source = tabSalades[rndSalade.Next(0, 5)];

                    // Démarrer le timer de pause
                    saladeTimer.Start();
                }
            }
        }
        private void BasculerMenuPause()
        {
            if (PauseMenu.Visibility == Visibility.Collapsed)
            {
                // Mettre en pause le jeu
                minuterie.Stop();
                PauseMenu.Visibility = Visibility.Visible;
            }
            else
            {
                // Reprendre le jeu
                minuterie.Start();
                PauseMenu.Visibility = Visibility.Collapsed;
            }
        }
        private void BoutonSon_Clic(object sender, RoutedEventArgs e)
        {
            // Logique pour gérer les paramètres sonores
            MessageBox.Show("Paramètres sonores à implémenter");
        }

        private void BoutonResolution_Clic(object sender, RoutedEventArgs e)
        {
            // Logique pour gérer la résolution
            MessageBox.Show("Paramètres de résolution à implémenter");
        }

        private void BoutonReprendre_Clic(object sender, RoutedEventArgs e)
        {
            BasculerMenuPause();
        }
    }
}