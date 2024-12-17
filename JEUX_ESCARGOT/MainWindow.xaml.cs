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
using System;

namespace JEUX_ESCARGOT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static BitmapImage escargotGauche;
        private static BitmapImage escargotDroit;
        private static BitmapImage escargotHaut;
        private static BitmapImage escargotBas;

        //Initialisation des timers
        private static DispatcherTimer minuterie;
        private DispatcherTimer saladeTimer;
        private DispatcherTimer voitureTimer;
        private DispatcherTimer voitureGaucheTimer;
        private DispatcherTimer vieFamilleTimer;
        private DispatcherTimer vieGrandParentsTimer;

        //Timers en attente
        private bool saladeEnAttente = false;
        private bool voitureEnAttente = false;
        private bool voitureGaucheEnAttente = false;

        //Initialisation des randoms
        private static Random rnd = new Random();
        private static Random rndSalade = new Random();
        private static Random rndVoiture = new Random();
        private static Random rndVoitureGauche = new Random();

        //Initialisation des variables/constantes
        private static bool gauche, droite, haut, bas, easterEgg;
        private static MediaPlayer sonDeFond;
        private static readonly int VITESSE_SALADE = 5;
        public int PAS_ESCARGOT = 10;
        private static int VITESSE_VOITURE = 5;
        private static int VITESSE_SOURIS = 1;
        int nbSalade = 0, barreDeVie = 3, score = 0;
        private const int MAX_VIE = 5;
        int vieFamille = 5;
        int vieGrandParents = 5;

        //Bouton quitter dans le menu (init ici car méthode de génération différente de celle des boutons de difficulté)
        private Button boutonQuitter;

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
        public void InitDebut()
        {
            vie3.Visibility = System.Windows.Visibility.Visible;
            vie2.Visibility = System.Windows.Visibility.Visible;
            nbSalade = 0;
            barreDeVie = 3;
            score = 0;
            l_score.Content = 0;
            droite = false;
            gauche = false;
            haut = false;
            bas = false;
            Canvas.SetTop(souris, 550);
            Canvas.SetLeft(souris, 1150);
        }
        // Méthode pour afficher le dialogue de difficulté
        private void AfficherDialogeDifficulte()
        {
            InitDebut();
            // Crée une boîte de dialogue pour le menu
            Window fenetreDialogue = new Window
            {
                Title = "Choisissez la difficulté",
                Width = 718,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri("pack://application:,,,/ressource/img/menu.png")),
                    Stretch = Stretch.UniformToFill
                }
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

                // Récupère l'index de difficulté et ferme le menu lors d'un clic sur l'un des trois boutons
                int indiceDifficulte = i;
                boutonDifficulte.Click += (s, e) => //Gestionnaire d'événement (event handler) ajouté à l'événement Click du bouton
                {
                    difficulteCourante = (Difficulte)indiceDifficulte; //Convertit indiceDifficulte en une valeur de l'enum Difficulte défini en haut
                    DefinirVitesse(difficulteCourante); // Défini la difficulté
                    fenetreDialogue.Close(); // Ferme le menu
                };
                panneau.Children.Add(boutonDifficulte);
            }

            boutonQuitter = new Button
            {
                Content = "Quitter",
                Width = 200,
                Height = 50,
                Margin = new Thickness(0, 10, 0, 0)
            };
            boutonQuitter.Click += BoutonQuitter_Click;

            panneau.Children.Add(boutonQuitter);
            fenetreDialogue.Content = panneau;
            fenetreDialogue.ShowDialog();
        }

        // La difficulté change la vitesse de l'escargot mais aussi celle des voitures
        private void DefinirVitesse(Difficulte difficulte)
        {
            switch (difficulte)
            {
                case Difficulte.Facile:
                    PAS_ESCARGOT = 10;
                    VITESSE_VOITURE = 3;
                    break;
                case Difficulte.Moyen:
                    PAS_ESCARGOT = 6;
                    VITESSE_SOURIS = 4;
                    break;
                case Difficulte.Difficile:
                    PAS_ESCARGOT = 15;
                    VITESSE_VOITURE = 10;
                    VITESSE_SOURIS = 10;
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
            escargotHaut = new BitmapImage(new Uri("pack://application:,,,/ressource/img/escargot-haut.png"));
            escargotBas = new BitmapImage(new Uri("pack://application:,,,/ressource/img/escargot-bas.png"));
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

        private void BoutonQuitter_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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
            else if (e.Key == Key.E)
            {
                easterEgg = false;
            }
        }

        private void Fenetre_TouchePressee(object sender, KeyEventArgs e)
        {
/*#if DEBUG
            Console.WriteLine(e.Key); // si besoin de debug si touche non detect
#endif*/
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
            else if (e.Key == Key.E)
            {
                easterEgg = true;
            }
        }

        private void Jeu(object? sender, EventArgs e)
        {
            l_nbSalade.Content = nbSalade;
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

            var sourisRect = new System.Drawing.Rectangle(
                (int)Canvas.GetLeft(souris),
                (int)Canvas.GetTop(souris),
                (int)souris.ActualWidth,
                (int)souris.ActualHeight);
            if (easterEgg == false)
            {
                MediaPlayer player = new MediaPlayer();
                player.Open(new Uri("chemin/vers/votre/tonnerre.mp3", UriKind.Relative));
                player.Play();
                if (sourisRect.IntersectsWith(familleRect) || sourisRect.IntersectsWith(grandParentsRect))
                {
                    Mort();
                }

                if (voitureRect.IntersectsWith(escargotRect) || voitureGaucheRect.IntersectsWith(escargotRect) || sourisRect.IntersectsWith(escargotRect))
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
                        Mort();
                    }
                }
            }

            if (saladeGaucheRect.IntersectsWith(escargotRect))
            {
                nbSalade++;
                Canvas.SetTop(imageSaladeGauche, 0 - imageSaladeGauche.ActualHeight);
                ReaparitionSalad();
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
                if (Canvas.GetLeft(escargot) > 1300 - escargotRect.Width)
                    Canvas.SetLeft(escargot, 1300 - escargotRect.Width);
            }
            if (bas)
            {
                escargot.Source = escargotBas;
                Canvas.SetTop(escargot, Canvas.GetTop(escargot) + PAS_ESCARGOT);
                if (Canvas.GetTop(escargot) > 700 - escargotRect.Height)
                    Canvas.SetTop(escargot, 700 - escargotRect.Height);
            }
            if (haut)
            {
                escargot.Source = escargotHaut;
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
                    ReaparitionSalad();

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
                    ReaparitionVoiture();

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
                    ReaparitionVoitureGauche();

                    // Démarrer le timer de pause UNIQUEMENT s'il n'est pas déjà en cours
                    if (!voitureGaucheTimer.IsEnabled)
                    {
                        voitureGaucheTimer.Start();
                    }
                }
            }
            DeplacementEnnemi();
        }

        private void ReaparitionSalad()
        {
            Canvas.SetTop(imageSaladeGauche, -imageSaladeGauche.ActualHeight);
            Canvas.SetLeft(imageSaladeGauche, rnd.Next((int)Canvas.GetLeft(route), (int)(Canvas.GetLeft(route) + route.ActualWidth - imageSaladeGauche.ActualHeight)));
            imageSaladeGauche.Source = tabSalades[rndSalade.Next(0, 5)];
        }

        private void ReaparitionVoiture()
        {
            Canvas.SetTop(voiture, this.ActualHeight + voiture.ActualHeight);
            voiture.Source = tabVoitures[rndVoiture.Next(0, 2)];
        }

        private void ReaparitionVoitureGauche()
        {
            Canvas.SetTop(voitureGauche, 0 - voitureGauche.ActualHeight);
            voitureGauche.Source = tabVoituresGauche[rndVoitureGauche.Next(0, 2)];
        }

        private void DeplacementEnnemi()
        {
            Canvas.SetLeft(souris, Canvas.GetLeft(souris) + Math.Sign(Canvas.GetLeft(escargot) - Canvas.GetLeft(souris)));
            Canvas.SetTop(souris, Canvas.GetTop(souris) + Math.Sign(Canvas.GetTop(escargot) - Canvas.GetTop(souris)));

        }

        private void BasculerMenuPause()
        {
            if (PauseMenu.Visibility == Visibility.Collapsed)
            {
                // Mettre en pause le jeu
                minuterie.Stop();
                vieFamilleTimer.Stop();
                vieGrandParentsTimer.Stop();
                PauseMenu.Visibility = Visibility.Visible;
            }
            else
            {
                // Reprendre le jeu
                minuterie.Start();
                vieFamilleTimer.Start();
                vieGrandParentsTimer.Start();
                PauseMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void BoutonSon_Clic(object sender, RoutedEventArgs e)
        {
            // Hide main pause menu buttons
            SonBoutton.Visibility = Visibility.Collapsed;
            ReglesBoutton.Visibility = Visibility.Collapsed;
            ReprendreBoutton.Visibility = Visibility.Collapsed;
            DifficulteBoutton.Visibility = Visibility.Collapsed;
            QuitterBoutton.Visibility = Visibility.Collapsed;

            // Show sound settings grid
            SoundSettingsGrid.Visibility = Visibility.Visible;

            // Set slider to current volume
            VolumeSlider.Value = sonDeFond.Volume;
        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Update background music volume
            if (sonDeFond != null)
            {
                sonDeFond.Volume = VolumeSlider.Value;
            }
        }

        private void BoutonRetourParametresSon_Clic(object sender, RoutedEventArgs e)
        {
            // Hide sound settings grid
            SoundSettingsGrid.Visibility = Visibility.Collapsed;

            // Restore main pause menu buttons
            SonBoutton.Visibility = Visibility.Visible;
            ReglesBoutton.Visibility = Visibility.Visible;
            ReprendreBoutton.Visibility = Visibility.Visible;
            DifficulteBoutton.Visibility = Visibility.Visible;
            QuitterBoutton.Visibility = Visibility.Visible;
        }

        private void BoutonReprendre_Clic(object sender, RoutedEventArgs e)
        {
            BasculerMenuPause();
        }

        private void BoutonRegles_Clic(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("L'objectif est de nourir les grands-parents et la famille escargot.\n\n" +
                "Il faut éviter les voitures et la souris. Si ces derniers touchent l'escargot, le joueur perd une vie symbolisée par les coeurs en haut à gauche.\n\n" +
                "La souris ne doit pas atteindre ni les grands-parents ni la famille sous peine de perdre instantanément.");
        }
        private void DifficulteBoutton_Clic(object sender, RoutedEventArgs e)
        {
            Mort();
        }

        private void DonnerSaladesAFamille()
        {
            vieFamille = Math.Min(vieFamille + nbSalade, MAX_VIE);
            pbFamille.Value = vieFamille;
            nbSalade = 0;
        }

        private void DonnerSaladesAuxGrandParents()
        {
            vieGrandParents = Math.Min(vieGrandParents + nbSalade, MAX_VIE);
            pbGrandParents.Value = vieGrandParents;
            nbSalade = 0;
        }

        private void DiminuerVieFamille(object? sender, EventArgs e)
        {
            vieFamille = Math.Max(vieFamille - 1, 0);
            pbFamille.Value = vieFamille;
            score += 1;
            l_score.Content = score;

            if (vieFamille == 0)
            {
                Mort();
            }
        }

        private void DiminuerVieGrandParents(object? sender, EventArgs e)
        {
            vieGrandParents = Math.Max(vieGrandParents - 1, 0);
            pbGrandParents.Value = vieGrandParents;
            score += 1;
            l_score.Content = score;

            if (vieGrandParents == 0)
            {
                Mort();
            }
        }
        private void Mort()
        {
            minuterie.Stop();
            vieFamilleTimer.Stop();
            vieGrandParentsTimer.Stop();
            InitDebut();
            sonDeFond.Stop();
            MessageBox.Show("Game Over", "Fin de partie", MessageBoxButton.OK, MessageBoxImage.Error);
            this.AfficherDialogeDifficulte();
        }
    }
}