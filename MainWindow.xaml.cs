using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Threading;

namespace WpfTraining
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Couleur du joueur
        private byte myRed = 0;
        private byte myGreen = 0;
        private byte myBlue = 0;

        private bool precedenteCollisionRed = false;
        private bool precedenteCollisionGreen = false;
        private bool precedenteCollisionBlue = false;

        //Délais d'apparition des ennemis
        private int delaiColonne1;
        private int delaiColonne2;
        private int delaiColonne3;

        //Vitesse des ennemis
        private int vitesseEnnemis = 1;

        //Vitesse du joueur
        private int vitesseJoueur = 10;

        //Ennemis
        private List<Rectangle> ennemis;

        //Score
        private int score = 0;

        //Vie
        private int vies = 5;

        //Liste des sorties
        private Ellipse[] sorties;

        //Couleurs des sorties
        private List<Color> couleursSorties = new List<Color>() {
            Color.FromArgb(255,255,0,0), //Rouge
            Color.FromArgb(255, 0, 255, 0), //Vert
            Color.FromArgb(255, 0, 0, 255) //Bleu
        };

        //RNG
        private Random randomNumberGenerator = new Random(unchecked((int)(DateTime.Now.Ticks)));

        //Timer d'IHM
        public DispatcherTimer IHMTimer { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            //Création du timer d'IHM
            IHMTimer = new DispatcherTimer();
            IHMTimer.Tick += new EventHandler(IHMTimer_Tick);
            IHMTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            IHMTimer.Start();

            //Initialisation des délais d'apparition des ennemis
            delaiColonne1 = randomNumberGenerator.Next(50);
            delaiColonne2 = randomNumberGenerator.Next(200);
            delaiColonne3 = randomNumberGenerator.Next(500);

            ennemis = new List<Rectangle>();

            //Couleur des sorties (aléatoires)
            Color cl1 = couleursSorties[randomNumberGenerator.Next(couleursSorties.Count)];
            SolidColorBrush scb1 = new SolidColorBrush(cl1);
            this.exit1.Fill = scb1;

            Color cl2 = couleursSorties[randomNumberGenerator.Next(couleursSorties.Count)];
            SolidColorBrush scb2 = new SolidColorBrush(cl2);
            this.exit2.Fill = scb2;

            Color cl3 = couleursSorties[randomNumberGenerator.Next(couleursSorties.Count)];
            SolidColorBrush scb3 = new SolidColorBrush(cl3);
            this.exit3.Fill = scb3;

            sorties = new Ellipse[] {
                                this.exit1,
                                this.exit2,
                                this.exit3
                            };

            //Initialisation du label de score
            this.lblScore.Content = string.Format("Score : {0}{2}Vies : {1}", this.score, this.vies, Environment.NewLine);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //****************************************************************
            //Mise en pause
            //****************************************************************
            if (e.Key == Key.Space)
            {
                if (this.vies > 0)
                {
                    this.IHMTimer.IsEnabled = !this.IHMTimer.IsEnabled;
                    this.lblPause.Visibility = this.IHMTimer.IsEnabled ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
                }
            }
        }

        private void IHMTimer_Tick(object sender, EventArgs e)
        {
            //****************************************************************
            //Cas du game over
            //****************************************************************
            if (this.vies < 0)
            {
                //Affichage d'un message au milieu de l'écran et arrêt du timer du jeu
                this.lblPause.Content = "GAME OVER" + Environment.NewLine + "Score : " + this.score;
                this.lblPause.Visibility = System.Windows.Visibility.Visible;
                this.IHMTimer.IsEnabled = false;
                return;
            }


            //****************************************************************
            //Déplacements et collisions du joueur
            //****************************************************************
            GereDeplacementJoueur();


            //****************************************************************
            //Apparition des ennemis
            //****************************************************************
            delaiColonne1--;
            delaiColonne2--;
            delaiColonne3--;

            if (delaiColonne1 <= 0)
            {
                //apparition d'un enemi
                AjouteEnnemi(150);

                delaiColonne1 = 500 + randomNumberGenerator.Next(100);
            }

            if (delaiColonne2 <= 0)
            {
                //apparition d'un enemi
                AjouteEnnemi(330);

                delaiColonne2 = 500 + randomNumberGenerator.Next(100);
            }

            if (delaiColonne3 <= 0)
            {
                //apparition d'un enemi
                AjouteEnnemi(510);

                delaiColonne3 = 500 + randomNumberGenerator.Next(100);
            }

            //****************************************************************
            //Déplacement des ennemis
            //****************************************************************
            List<Rectangle> copieEnnemis = ennemis;
            for (int i = 0; i < copieEnnemis.Count; i++)
            {
                Rectangle ennemi = copieEnnemis[i];

                //Déplacement de l'ennemi vers le bas de l'écran
                ennemi.Margin = new Thickness(ennemi.Margin.Left, ennemi.Margin.Top + vitesseEnnemis, ennemi.Margin.Right, ennemi.Margin.Bottom);

                //Quand l'ennemi franchit le bas de l'écran, on l'enlève et on vérifie la couleur de la sortie
                if (ennemi.Margin.Top + ennemi.Height > this.Height - exit1.Height)
                {
                    ennemis.RemoveAt(i);
                    this.mainGrid.Children.Remove(ennemi);

                    //vérification de la collision
                    foreach (Ellipse sortie in this.sorties)
                    {
                        if (Collision(ennemi, sortie))
                        {
                            if (ennemi.Fill.ToString() == sortie.Fill.ToString())
                            {
                                this.score += 100;

                                if (this.score % 1000 == 0)
                                {
                                    this.vies++; //Une vie supplémentaire tous les 1000 points

                                    if (this.score > 1000)
                                        this.vitesseEnnemis = (int)(this.score / 1000);
                                }

                                //Ajout des couleurs de sorties
                                if (score == 1000)
                                    this.couleursSorties.Add(Color.FromRgb(255, 255, 0));

                                if (score == 1500)
                                    this.couleursSorties.Add(Color.FromRgb(255, 0, 255));

                                if (score == 2000)
                                    this.couleursSorties.Add(Color.FromRgb(0, 255, 255));

                                if (score == 2500)
                                    this.couleursSorties.Add(Color.FromRgb(255, 255, 255));

                                if (score == 3000)
                                    this.couleursSorties.Add(Color.FromRgb(0, 0, 0));
                            }
                            else
                            {
                                this.vies--;
                            }
                            this.lblScore.Content = string.Format("Score : {0}{2}Vies : {1}", this.score, this.vies, Environment.NewLine);

                            //Changement de couleur de la sortie touchée
                            sortie.Fill = new SolidColorBrush(couleursSorties[randomNumberGenerator.Next(couleursSorties.Count)]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gere le déplacement et les collisions du joueur
        /// </summary>
        private void GereDeplacementJoueur()
        {
            //****************************************************************
            //Déplacement du sprite du joueur
            //****************************************************************
            int vitesseX = 0;
            int vitesseY = 0;

            //Calcul de la vitesse verticale
            if (Keyboard.IsKeyDown(Key.Up))
                vitesseY = -vitesseJoueur;
            else if (Keyboard.IsKeyDown(Key.Down))
                vitesseY = vitesseJoueur;

            //Calcul de la vitesse horizontale
            if (Keyboard.IsKeyDown(Key.Left))
                vitesseX = -vitesseJoueur;
            else if (Keyboard.IsKeyDown(Key.Right))
                vitesseX = vitesseJoueur;

            //Déplacement
            this.player.Margin = new Thickness(this.player.Margin.Left + vitesseX, this.player.Margin.Top + vitesseY, this.player.Margin.Right, this.player.Margin.Bottom);

            //****************************************************************
            //Maintenir le joueur dans les limites de l'écran
            //****************************************************************
            if (this.player.Margin.Left <= 0)
                this.player.Margin = new Thickness(0, this.player.Margin.Top, this.player.Margin.Right, this.player.Margin.Bottom);

            if (this.player.Margin.Left >= this.Width - this.player.Width)
                this.player.Margin = new Thickness(this.Width - this.player.Width, this.player.Margin.Top, this.player.Margin.Right, this.player.Margin.Bottom);

            if (this.player.Margin.Top <= 0)
                this.player.Margin = new Thickness(this.player.Margin.Left, 0, this.player.Margin.Right, this.player.Margin.Bottom);

            if (this.player.Margin.Top >= this.Height - this.player.Height)
                this.player.Margin = new Thickness(this.player.Margin.Left, this.Height - this.player.Height, this.player.Margin.Right, this.player.Margin.Bottom);


            //****************************************************************
            //Collisions avec les pots de peinture
            //****************************************************************

            //Rouge
            if (Collision(this.player, this.lblR))
            {
                if (!precedenteCollisionRed)
                {
                    myRed = myRed == 0 ? (byte)255 : (byte)0;

                    SolidColorBrush scb = new SolidColorBrush();
                    scb.Color = Color.FromArgb(255, myRed, myGreen, myBlue);
                    this.player.Fill = scb;

                    precedenteCollisionRed = true;
                }
            }
            else
                precedenteCollisionRed = false;

            //Vert
            if (Collision(this.player, this.lblG))
            {
                if (!precedenteCollisionGreen)
                {
                    myGreen = myGreen == 0 ? (byte)255 : (byte)0;

                    SolidColorBrush scb = new SolidColorBrush();
                    scb.Color = Color.FromArgb(255, myRed, myGreen, myBlue);
                    this.player.Fill = scb;

                    precedenteCollisionGreen = true;
                }
            }
            else
                precedenteCollisionGreen = false;

            //Bleu
            if (Collision(this.player, this.lblB))
            {
                if (!precedenteCollisionBlue)
                {
                    myBlue = myBlue == 0 ? (byte)255 : (byte)0;

                    SolidColorBrush scb = new SolidColorBrush();
                    scb.Color = Color.FromArgb(255, myRed, myGreen, myBlue);
                    this.player.Fill = scb;

                    precedenteCollisionBlue = true;
                }
            }
            else
                precedenteCollisionBlue = false;


            //****************************************************************
            //Collisions avec les ennemis
            //****************************************************************
            foreach (Rectangle ennemy in ennemis)
            {
                if (Collision(this.player, ennemy))
                {
                    ennemy.Fill = this.player.Fill;
                }
            }

        }


        /// <summary>
        /// Ajoute un ennemi en haut de l'écran, à l'abcisse passée en paramètre
        /// </summary>
        /// <param name="abcisse"></param>
        private void AjouteEnnemi(int abcisse)
        {
            Rectangle ennemi = new Rectangle();

            ennemi.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            ennemi.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            ennemi.Width = 50;
            ennemi.Height = 50;

            SolidColorBrush scb = new SolidColorBrush();
            scb.Color = Color.FromArgb(255, 255, 255, 255);
            ennemi.Fill = scb;

            ennemi.Margin = new Thickness(abcisse, -50, 0, 0);

            this.mainGrid.Children.Add(ennemi);

            ennemis.Add(ennemi);
        }

        /// <summary>
        /// Vérifie une collision simple en 2D entre 2 formes
        /// </summary>
        /// <param name="a">1ere forme à tester</param>
        /// <param name="b">2e forme à tester</param>
        /// <returns>True si les deux formes se touchent, False sinon.</returns>
        public bool Collision(Shape a, Shape b) 
        {
            return !(a.Margin.Top + a.Height < b.Margin.Top
                || a.Margin.Top > b.Margin.Top + b.Height
                || a.Margin.Left + a.Width < b.Margin.Left
                || a.Margin.Left > b.Margin.Left + b.Width);
        }
    }
}
