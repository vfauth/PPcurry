class Game
{
	// Attributes

	private Card firstCardSelected; // The first selected card this turn
	private Board board; // The board with all the cards
	private Display display; // The GUI

	
	// Accessors/Mutators

	public Board GetBoard()


	// Constructor

	/// Create a new game
	public Game(Display display, int X, int Y)

	
	// Methods

	/// Method to be executed when a card is chosen
	public void CardChosen(object sender, EventArgs e)

	/// Go to the next turn by incrementing the counter and refreshing the display
	public void NextTurn()

	/// Check whether two cards are paired
	private bool CheckPair(Card card1, Card card2)

	/// Check whether the game is finished or not
	private bool IsFinished()

	/// Exit the game
	public void Exit(object sender)
	
}
