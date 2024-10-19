# GomokuComp
## A package for creating and testing Gomoku bots in use for the GomokuComp Competition

### Rules 
- The Gomoku variant used in this comp is unrestricted Gomoku, where there are no special rules for starting play.
- For the purpose of approaching creative solutions, you may not look up solutions online for algorithms, theorems, or any of the sort (honor system). Since unrestricted Gomoku is solved, this would sort of defeat the purpose.
- Teams are capped at 2 maximum, and individual submissiona are allowed

### Setup and Usage
- Clone the repository to your Visual Studio or VSCode.

# How to use GomokuPackage (also in the GomokuPackage folder)

## Classes
- **RandomBot**: This is a template for you to implement your own bot. It uses the IBot interface, to ensure a MakeMove method is used. You must also rename the class, and also change it's Name property to ensure it runs properly in the tournament.
- **Program**: This is how you will test your bot. The first line in the Main method provides a list of IBots. To test any amount of bots, add any amount of valid IBots to that list, and the program will run a simple swiss tournament for all of the bots, and display the number of wins afterwards.

### DO NOT MODIFY THE FOLLOWING CLASSES
- **Tournament**: This class handles the tournament, as well as enforcing legal moves and time limits. Each player has only 1 millisecond per move.
- **IBot**: This is the IBot interface
- **GomokuGame**: This class handles each individual game of Gomoku, played in the unrestricted variant. 
