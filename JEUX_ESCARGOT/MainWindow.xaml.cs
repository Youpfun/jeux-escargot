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
using System.Data;

namespace JEUX_ESCARGOT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int PAS_ESCARGOT = 10;
        private static BitmapImage escargotGauche;
        private static BitmapImage escargotDroit;
        private static DispatcherTimer minuterie;
        private DispatcherTimer saladeTimer;
        private DispatcherTimer voitureTimer;
        private DispatcherTimer voitureGaucheTimer;
        private DispatcherTimer vieFamilleTimer;
        private DispatcherTimer vieGrandParentsTimer;
        private bool saladeEnAttente = false;
        private bool voitureEnAttente = false;
        private bool voitureGaucheEnAttente = false;
        private static Random rnd = new Random();
        private static Random rndSalade = new Random();
        private static Random rndVoiture = new Random();
        private static Random rndVoitureGauche = new Random();
        private static bool gauche, droite, haut, bas;
        private static MediaPlayer sonDeFond;
        private static readonly int VITESSE_SALADE = 5;
        private static int VITESSE_VOITURE = 5;
        int score = 0, barreDeVie = 3;
        int vieFamille = 5;
        int vieGrandParents = 5;
        
        // Tableaux contenant les adresses des images pour pouvoir les faire varier lors des spawn
        System.Windows.Media.ImageSource[] tabSalades = new System.Windows.Media.ImageSource[]
        {
            new BitmapImage(new Uri("pack://application:,,,/ressource/img/salade1bg.png")),
            new BitmapImage(new Uri("pack://application:,,,/ressource/img/salade2bg.png")),
            new BitmapImage(new Uri("pack://application:,,,/ressource/img/salade3bg.png")),
            new BitmapImage(new Uri("pack://application:,,,/ressource/img/salade4bg.png")),
            new BitmapImage(new Uri("pack://application:,,,/ressource/img/salade5bg.png")),
            new BitmapImage(new Uri("pack://application:,,,/ressource/img/salade6bg.png"))
        };
        System.Windows.Media.ImageSource[] tabVoitures = new System.Windows.Media.ImageSource[]
        {
            new BitmapImage(new Uri("pack://application:,,,/ressource/img/voiture1.png")),
            new BitmapImage(new Uri("pack://application:,,,/ressource/img/voiture3.png"))
        };
        System.Windows.Media.ImageSource[] tabVoituresGauche = new System.Windows.Media.ImageSource[]
        {
            new BitmapImage(new Uri("pack://application:,,,/ressource/img/voiture1Retournee.png")),
            new BitmapImage(new Uri("pack://application:,,,/ressource/img/voiture3Retournee.png"))
        };

        
        //Permet de créer un type de donnée avec ses différentes valeurs possibles (ici : Facile, Moyen, Difficile)
        public enum Difficulte
        {
            Facile,
            Moyen,
            Difficile
        }

        private Difficulte difficulteCourante = Difficulte.Moyen; // Difficulté par défaut (il en faut une sinon erreur)

        public MainWindow()
        {
            InitializeComponent();
            AfficherDialogeDifficulte();
        }

        // Méthode pour afficher le dialogue de difficulté
        private void AfficherDialogeDifficulte()
        {
            // Créer une boîte de dialogue pour le menu
            Window fenetreDialogue = new Window
            {
                Title = "Choisissez la difficulté",
                Width = 300,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel panneau = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock titre = new TextBlock
            {
                Text = "Sélectionnez la difficulté",
                FontSize = 18,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            panneau.Children.Add(titre);

            // Boutons de difficulté
            string[] niveauxDifficulte = { "Facile", "Moyen", "Difficile" };
            for (int i = 0; i < niveauxDifficulte.Length; i++)
            {
                Button boutonDifficulte = new Button
                {
                    Content = niveauxDifficulte[i],
                    Width = 200,
                    Height = 50,
                    Margin = new Thickness(0, 10, 0, 0)
                };

                // Capture de l'index pour la fermeture
                int indiceDifficulte = i;
                boutonDifficulte.Click += (s, e) =>
                {
                    difficulteCourante = (Difficulte)indiceDifficulte;
                    DefinirVitesseEscargot(difficulteCourante);
                    fenetreDialogue.Close();
                };

                panneau.Children.Add(boutonDifficulte);
            }

            fenetreDialogue.Content = panneau;
            fenetreDialogue.ShowDialog();
        }

        // La difficulté change la vitesse de l'escargot mais aussi celle des voitures
        private void DefinirVitesseEscargot(Difficulte difficulte)
        {
            switch (difficulte)
            {
                case Difficulte.Facile:
                    PAS_ESCARGOT = 10;
                    break;
                case Difficulte.Moyen:
                    PAS_ESCARGOT = 6;
                    break;
                case Difficulte.Difficile:
                    PAS_ESCARGOT = 15;
                    VITESSE_VOITURE = 10;
                    break;
            }

            InitBitmaps();
            InitTimer();
            InitMusique();
            InitSaladeTimer();
            InitVoitureTimer();
            InitVoitureGaucheTimer();
            InitVieFamilleTimer();
            InitVieGrandParentsTimer();
            InitVieFamille();
            InitVieGrandParents();
        }

        private void InitSaladeTimer()
        {
            saladeTimer = new DispatcherTimer();
            saladeTimer.Interval = TimeSpan.FromSeconds(3);
            saladeTimer.Tick += ReprendreDescente;
        }

        private void InitVoitureTimer()
        {
            voitureTimer = new DispatcherTimer();
            voitureTimer.Interval = TimeSpan.FromSeconds(rnd.Next(0, 5));
            voitureTimer.Tick += ReprendreDescenteVoiture;
        }

        private void InitVoitureGaucheTimer()
        {
            voitureGaucheTimer = new DispatcherTimer();
            voitureGaucheTimer.Interval = TimeSpan.FromSeconds(rnd.Next(0, 5));
            voitureGaucheTimer.Tick += ReprendreDescenteVoitureGauche;
        }

        private void InitVieFamilleTimer()
        {
            vieFamilleTimer = new DispatcherTimer();
            vieFamilleTimer.Interval = TimeSpan.FromSeconds(5);
            vieFamilleTimer.Tick += DiminuerVieFamille;
            vieFamilleTimer.Start();
        }

        private void InitVieGrandParentsTimer()
        {
            vieGrandParentsTimer = new DispatcherTimer();
            vieGrandParentsTimer.Interval = TimeSpan.FromSeconds(5);
            vieGrandParentsTimer.Tick += DiminuerVieGrandParents;
            vieGrandParentsTimer.Start();
        }

        private void InitVieFamille()
        {
            vieFamille = 5;
            pbFamille.Value = vieFamille;
        }

        private void InitVieGrandParents()
        {
            vieGrandParents = 5;
            pbGrandParents.Value = vieGrandParents;
        }

        private void ReprendreDescente(object? sender, EventArgs e)
        {
            // Arrêter le timer
            saladeTimer.Stop();

            // Reprendre la descente
            saladeEnAttente = false;
        }

        private void ReprendreDescenteVoiture(object? sender, EventArgs e)
        {
            voitureTimer.Stop();
            voitureEnAttente = false;
        }

        private void ReprendreDescenteVoitureGauche(object? sender, EventArgs e)
        {
            voitureGaucheTimer.Stop();
            voitureGaucheEnAttente = false;
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

        public static void InitMusique()
        {
            sonDeFond = new MediaPlayer();
            sonDeFond.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/ressource/son/test.mp3"));
            sonDeFond.MediaEnded += RelanceMusique;
            sonDeFond.Volume = 1;
            sonDeFond.Play();
        }

        private static void RelanceMusique(object? sender, EventArgs e)
        {
            sonDeFond.Position = TimeSpan.Zero;
            sonDeFond.Play();
        }

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
            l_score.Content = score;
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

            var voitureGaucheRect = new System.Drawing.Rectangle(
                (int)Canvas.GetLeft(voitureGauche),
                (int)Canvas.GetTop(voitureGauche),
                (int)voitureGauche.ActualWidth,
                (int)voitureGauche.ActualHeight);

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

            var familleRect = new System.Drawing.Rectangle(
                (int)Canvas.GetLeft(familleEscargot),
                (int)Canvas.GetTop(familleEscargot),
                (int)familleEscargot.ActualWidth,
                (int)familleEscargot.ActualHeight);

            var grandParentsRect = new System.Drawing.Rectangle(
                (int)Canvas.GetLeft(gpEscargot),
                (int)Canvas.GetTop(gpEscargot),
                (int)gpEscargot.ActualWidth,
                (int)gpEscargot.ActualHeight);

            if (voitureRect.IntersectsWith(escargotRect) || voitureGaucheRect.IntersectsWith(escargotRect))
            {
                barreDeVie--;
                Canvas.SetLeft(escargot, 0);
                if (barreDeVie == 2)
                {
                    vie3.Visibility = System.Windows.Visibility.Collapsed;
                }
                else if (barreDeVie == 1)
                {
                    vie2.Visibility = System.Windows.Visibility.Collapsed;
                }
                else //(barreDeVie == 0)
                {
                    MessageBox.Show("Game Over");
                    this.Close();
                }
            }

            if (saladeGaucheRect.IntersectsWith(escargotRect))
            {
                score++;
                Canvas.SetTop(imageSaladeGauche, 0 - imageSaladeGauche.ActualHeight);
                RespawnSalad();
            }

            if (escargotRect.IntersectsWith(familleRect))
            {
                DonnerSaladesAFamille();
            }

            if (escargotRect.IntersectsWith(grandParentsRect))
            {
                DonnerSaladesAuxGrandParents();
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
                if (Canvas.GetLeft(escargot) > this.ActualWidth - escargotRect.Width)
                    Canvas.SetLeft(escargot, this.ActualWidth - escargotRect.Width);
            }
            if (bas)
            {
                Canvas.SetTop(escargot, Canvas.GetTop(escargot) + PAS_ESCARGOT);
                if (Canvas.GetTop(escargot) > this.ActualHeight - escargotRect.Height + PAS_ESCARGOT)
                    Canvas.SetTop(escargot, this.ActualHeight - escargotRect.Height + PAS_ESCARGOT);
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
                    RespawnSalad();

                    // Démarrer le timer de pause
                    saladeTimer.Start();
                }
            }

            if (!voitureEnAttente)
            {
                Canvas.SetTop(voiture, Canvas.GetTop(voiture) - VITESSE_VOITURE);

                // Vérifier si la voiture est sortie de l'écran
                if (Canvas.GetTop(voiture) < 0 - voiture.ActualHeight)
                {
                    // Mettre en pause la descente
                    voitureEnAttente = true;

                    // Positionner la voiture au-dessus de l'écran
                    RespawnVoiture();

                    // Démarrer le timer de pause UNIQUEMENT s'il n'est pas déjà en cours
                    if (!voitureTimer.IsEnabled)
                    {
                        voitureTimer.Start();
                    }
                }
            }
            if (!voitureGaucheEnAttente)
            {
                Canvas.SetTop(voitureGauche, (Canvas.GetTop(voitureGauche) + VITESSE_VOITURE));

                // Vérifier si la voiture est sortie de l'écran
                if (Canvas.GetTop(voitureGauche) > this.ActualHeight + voitureGauche.ActualHeight)
                {
                    // Mettre en pause la descente
                    voitureGaucheEnAttente = true;

                    // Positionner la voiture au-dessus de l'écran
                    RespawnVoitureGauche();

                    // Démarrer le timer de pause UNIQUEMENT s'il n'est pas déjà en cours
                    if (!voitureGaucheTimer.IsEnabled)
                    {
                        voitureGaucheTimer.Start();
                    }
                }
            }
        }

        private void RespawnSalad()
        {
            Canvas.SetTop(imageSaladeGauche, -imageSaladeGauche.ActualHeight);
            Canvas.SetLeft(imageSaladeGauche, rnd.Next((int)Canvas.GetLeft(route), (int)(Canvas.GetLeft(route) + route.ActualWidth - imageSaladeGauche.ActualHeight)));
            imageSaladeGauche.Source = tabSalades[rndSalade.Next(0, 5)];
        }

        private void RespawnVoiture()
        {
            Canvas.SetTop(voiture, this.ActualHeight + voiture.ActualHeight);
            voiture.Source = tabVoitures[rndVoiture.Next(0, 2)];
        }

        private void RespawnVoitureGauche()
        {
            Canvas.SetTop(voitureGauche, 0 - voitureGauche.ActualHeight);
            voitureGauche.Source = tabVoituresGauche[rndVoitureGauche.Next(0, 2)];
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

        private void DonnerSaladesAFamille()
        {
            vieFamille = Math.Min(vieFamille + score, 5);
            pbFamille.Value = vieFamille;
            score = 0;
        }

        private void DonnerSaladesAuxGrandParents()
        {
            vieGrandParents = Math.Min(vieGrandParents + score, 5);
            pbGrandParents.Value = vieGrandParents;
            score = 0;
        }

        private void DiminuerVieFamille(object? sender, EventArgs e)
        {
            vieFamille = Math.Max(vieFamille - 1, 0);
            pbFamille.Value = vieFamille;

            if (vieFamille == 0)
            {
                MessageBox.Show("Game Over");
                this.Close();
            }
        }

        private void DiminuerVieGrandParents(object? sender, EventArgs e)
        {
            vieGrandParents = Math.Max(vieGrandParents - 1, 0);
            pbGrandParents.Value = vieGrandParents;

            if (vieGrandParents == 0)
            {
                MessageBox.Show("Game Over");
                this.Close();
            }
        }
    }
}