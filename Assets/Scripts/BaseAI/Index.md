# BaseAI Development Guide (v0.1.1)

This document is a short guide on the usage of the BaseAI component for the development of competitive racing AI within Race Baby Race.

## Methods

- **Void** SetDirection(**Vector2** newDirection)

  > Sets a new direction for the BaseAI to head in.
  >
  > newDirection.x Specifies the desired steering angle and may range from -1 to 1.
  >
  > newDirection.y Specifies the desired speed and may range from -1 to 1.

- **Vector3** GetFirstNode()

  > Returns the First node at the startline. Each node contains a variable containing its adjacent node(s).
  
- **Vector3** GetPlayerPositions()

  > Gets an array of player positions.
  
- **Item** GetCurrentItem()

  > Gets the current item the player is holding.

- **Void** AimBack(bool a)

  > Sets the direction for the car to aim in.

- **Void** UseItem()

  > Uses the current item
  >

- **Void** Respawn()

  > Respawns the player to the most recent checkpoint.
  >
  
- **Void** SetName()

  > Sets the name of the AI.
  >

- **Void** SetBody(CarBody newBody)

  > Sets the body of the AI.
  >

## Variables

- **int** position

  > Contains the current position of the AI in the race. This int is assigned by the RaceManager.

  **Node** nextNodes

  > A Node's subsequent node(s).
  >
  > **You may only read the next node's position**

## Todo

- Method for using items (finish)
- Visual for items

