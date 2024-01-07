# On-The-Waves
*BOATS DO VZHHHHH BBBDROOM, BEEEEP, BEEEP, GNAA, HONK, VZHHHHHHHHHHHHHH*

Well, this is my bachelor's thesis on the topic "Development of a multiplayer racing game with the use of artificial intelligence for simulating opponents' behavior."

As part of this work, I've analyzed existing competitive games with sym-metrical gameplay, which contain AI, simulating the behavior of opponents, and used them to determine the criteria for comparison. The performance of AI models trained in different ways was investigated. Based on this analysis, the final models were selected and a multiplayer game in the "Racing" genre, based on the game engine Unity, which meets the criteria were developed, as well as the directions for further research. The game is cross-platform without limiting the game session by the device type and is available for Windows, Linux, MacOS, Android and iOS operating systems.

As the primary development tool for the project, the Unity game engine was chosen (programming language – C#). The Netcode for GameObjects plugin was selected as the solution for implementing the multiplayer mode. Additionally, services specialized for the chosen game engine provided by Unity Gaming Services were utilized, namely:

- Lobby Service for initially grouping players in a single lobby, preconfiguring connection settings, and race parameters.
- Relay Service for organizing communication between players and determining their network interactions during gameplay.

To implement opponent behavior models, the ml-agents package, specifically designed for the Unity game engine, was employed. This package enables the training of intelligent agents through reinforcement learning and imitation learning using algorithms such as PPO, MA-POCA, and SAC.

**ML agents Comparisson:**
![image](https://github.com/ImSOLty/On-The-Waves/assets/48078801/5d645a07-f8c9-455e-8080-49ea3fabb73c)


**Trajectories of different ML agents:**
![Trajectories](https://github.com/ImSOLty/On-The-Waves/assets/48078801/1d4a4e73-aa0d-4f30-8f98-fd26818ad9a9)


**Screenshots:**
![Location](https://github.com/ImSOLty/On-The-Waves/assets/48078801/9cdc0910-1462-4f21-8ceb-720ed8f54bb7)
![Main Menu](https://github.com/ImSOLty/On-The-Waves/assets/48078801/0c970cd8-7715-420e-bebf-b7b4aa2fdb72)
![Lobby Menu](https://github.com/ImSOLty/On-The-Waves/assets/48078801/ea6d23d5-f525-4350-970e-f9a9bbead48f)
![Ingame](https://github.com/ImSOLty/On-The-Waves/assets/48078801/233e863d-e94f-424b-a6b4-70a4a00f669a)

**Videos:**
- Agent Trajectories: https://youtu.be/mG_ogb8pcBU
- On The Waves DEMO: https://youtu.be/QV4JaJz_RGs

**Game (itch.io):** https://imsolty.itch.io/on-the-waves

**WaveCreator:** https://imsolty.itch.io/wavecreator
