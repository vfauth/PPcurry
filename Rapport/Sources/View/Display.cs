class Display
{
	// Attributes
	private MainWindow mainWindow; // The window
	private Game game; // The controller
	private Board board; // The board
	private Card[][] cards; // The cards
	private Image[][] images; // The objects used to display the cards in WPF 

	private BitmapImage[] symbols; // Pictures to show on the cards
	BitmapImage hidden; // Picture to show on hidden cards

	public const int CARD_X = 127; // Width (in pixels) of every card
	public const int CARD_Y = 200; // Height (in pixels) of every card


	// Constructor

	/// Create a graphical interface for the game and initialize the game itself
	public Display(MainWindow mainWindow, int X, int Y)
	

	// Methods

	/// Load all the images present in the ./resources/cards directory
	public void LoadImages(int number)

	/// Create the graphical controls for the board and the cards
	public void CreateBoard()

	/// Update the display
	public void PrintBoard()

	/// Create an image
	public Image CreateCard(Panel panel, int x, int y, int width, int height, int marginLeft = 0, int marginTop = 0, int marginRight = 0, int marginBottom = 0)

	/// Display a pop-up with the number of turns
	public void PrintTurns()
	
}
