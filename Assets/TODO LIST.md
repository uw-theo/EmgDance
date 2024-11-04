GAME TODO LIST
Misc links that might be helpful
https://github.com/balandinodidonato/MyoToolkit/blob/master/Software%20for%20Thalmic%27s%20Myo%20armband.md
https://github.com/thalmiclabs/myo-unity/blob/master/project/Assets/Myo%20Samples/Scripts/JointOrientation.cs
https://github.com/charliegerard/myo-web-bluetooth.js
http://web.archive.org/web/20180702010958/https://developer.thalmic.com/start/
http://web.archive.org/web/20220518055600/https://developerblog.myo.com/new-in-myo-connect-gesture-overlay-pointer-color-and-more/
http://web.archive.org/web/20220802123502/https://developerblog.myo.com/setting-myo-package-unity/
https://web.archive.org/web/20220516115638/https://developerblog.myo.com/getting-started-myo-scripts-part-2/

https://github.com/NiklasRosenstein/myo-python/releases/tag/v1.0.4


[X]1. get counter for each gesture separately for left, right, up, and down
[X]2. display statistics for each of the gestures at the end of the game. includes: 
    a. left, right, up, down hit
    b. left, right, up, down miss
    c. total score
[X]3. At the end of the game, write out all statistics out to a CSV file. Maybe JSON? 
    a. [x]date
    b. [x]user id
    c. [x]song name
    d. [x]total note hit vs miss
    e. [x]left note hit vs miss
    f. [x]right note hit vs miss
    g. [x]up note hit vs miss
    h. [x]down note hit vs miss
    i. [x]final score (each hit gives static increase, no combo increase)
[X]4. Most common error
    a. display this either to the ending screen and/or log to the csv file
[~]5. (OPTIONAL)history scene
    a. [x]ability to load which song to look at
    b. [x]when clicking on box, it displays the relevant information in graph format across time
    c. [x]update to have the X and Y axis labelled and fits with the data properly
    d. [~]edit graphing area so that labels show up properly
[X]6. double check that Myo controls still work after everything
[]7. (OPTIONAL)(JUST ONE SONG)Have 2 songs that work
    a. Prelude is used for actual runs
    b. [x]Goin' Under is used for tutorial
[]8. (NICE TO HAVE)Double check that the song and note timing work out properly
[]9. (NICE TO HAVE)Tighten sensitivity for note counted as hits. Sometimes still feels a little bit off. 
[]10. (NICE TO HAVE)Prelude song doesn't work at all right now. Notes disappear into the floor. 
    a. might have to do with the new equation i tried to make the notes move differently or more accurately? not sure. 
[]11. Create an excel sheet that tracks the 20 random gestures for left and 20 random gestures for right foot. 
[X]12. replace arrow picture with a foot facing left or right and associtated gesture
[]13. find a good way to measure the top of the leg and where on the calf the myo band is
    a. also measure from the top of the myo band
[X]14. change the color of the left gestures to LAVENDER and the right gestures to RED
[]15. start writing a script on how the game will be run
[]16. (OPTIONAL) change the foot to be a wooden or metal foot, not realistic looking at all
[X]17. After 10 seconds, automatically go back to the song select menu
[X]18. sometimes Myos seem to flip legs so that right is left and left is right gestures. Any way to fix? 
    not able to use by unique name
    there is an older myo that has an older firmware version. We can trigger out of that. 
[X]19. Make sure i can create a binary of the final build. The final build needs to have the following from the "Builds" folder. 
    DdrTutorial.exe file
    UnityPlayer.dll file
    DdrTutorial_data folder
        need to add in "Songs" folder with "Goin' Under" song
        need to add in "UserProfiles" folder
    MonoBleedingEdge folder
[]20. Cleanup the code, rename the project, etc. Get this thing finalized!!!

Immediate Goal: 
items 9, 19, 11, 13, 15, 20
final code clean up and refactor phase. Get ready for actual test subjects ASAP. 


PAPER TODO LIST
Overall goal: 
i want to create a platform that can be used as a basis for future experiements.
to run this experiment on healthy patients first so that i can learn things abouts useability that would affect amputees. 

how do i want to start my abstract?
introduce with "advances in VR, semg, etc technologies? 
introduce with what is PLP and how we want to hit that? 
introduce with statistics about upper vs lower limb

abstract should begin with a brief statemtn of the problem or issue, followed by a description of the research method and design, the major findings, and the conclusions reached. 
1. a clear statement of the topic of the paper and a little motivation
2. a bit of an intro to draw a reader into wanting to know more
3. a clear and precise statement of what the paper is going to cover
4. a concise summary of the results of the analysis and the main conclusions


Deadlines:
checklist: https://grad.uw.edu/current-students/enrollment-through-graduation/graduation-requirements/

1. Two weeks before the defense, submit the title and abstract to the School of STEM Office of Graduate Studies to post on the Final Examination & Defense schedule.
    title page
    copyright page
    abstract
    Friday, 22nd November 2024
2. At least seven days before the defense, the student must submit a final draft of their project paper or thesis to their committee for a preliminary reading.
    Friday, 29th November 2024 
3. Final defense on Friday, December 6, 2024 at 1:15 PM 

