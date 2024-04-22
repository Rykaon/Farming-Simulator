INCLUDE globals.ink
EXTERNAL PlantSellBuy(PlantToBuy, PlantPrice, SellOrBuy)
EXTERNAL CheckRun()

{ IsBonus>true  : ->fin | ->debut}

=== fin ===
Voulez-vous finir la partie ? #speaker:Narrateur #portrait:Narrateur #layout:left
-> event

=== event ===
    + [Oui]
        Est-tu sûr ? Tu peux toujours passer au marchand pour avoir {NbrReward} $. #speaker:Narrateur #portrait:Narrateur #layout:left
        ++ [Oui]
            -> FinPartie
        ++ [Non]
        -> END
    + [Non]
        -> END

=== FinPartie ===
    {NbrReward <= PlayerArgent :
               Tu as gagné ... #speaker:Narrateur #portrait:Narrateur #layout:left
        - else :
               Tu as perdu ... #speaker:Narrateur #portrait:Narrateur #layout:left
    }
    ~ CheckRun()
    -> END
    
=== debut ===
Bonjour et bienvenue ! #speaker:Narrateur #portrait:Narrateur #layout:left
Votre objectif est d'atteindre {NbrReward}$ d'ici la fin de votre voyage ! #speaker:Narrateur #portrait:Narrateur #layout:left
N'oubliez pas d'acheter des plantes en cas de besoin pour vous défendre ! #speaker:Narrateur #portrait:Narrateur #layout:left
Bonne chance !
->END