INCLUDE globals.ink
EXTERNAL StealGive(RewardType, NbrReward, IsBonus)
->main


=== main ===
Vous entendez un bruit dehors. Une personne veut rentrer. #speaker:Narrateur #portrait:Narrateur #layout:left
-> event

=== event ===
    + [Le laisser rentrer]
        -> letIn 
    + [Le laisser dehors]
        -> letOut

=== letIn  ===
    Vous le laisser rentrer. #speaker:Narrateur #portrait:Narrateur #layout:left
    Hello ! #speaker:Inconnu #portrait:ManOutside #layout:right
            * [Bonjour !]
            * [Coucou !]
    --> monGrosBool(RewardType, NbrReward, IsBonus)

=== monGrosBool (Reward, Nbr, Bonus)===
La personne s'approche de vous.  #speaker:Narrateur #portrait:Narrateur #layout:left
            {Bonus == false :
                {Reward: 
                - "Gold":
                    {Nbr<PlayerArgent:
                        Je veux te voler de l'argent, merci. #speaker:Inconnu #portrait:ManOutside #layout:right
                    - else :
                        Je voulais te voler de l'argent, t'en a pas assez. #speaker:Inconnu #portrait:ManOutside #layout:right
                    }
                - "Attack":
                    {NbPlant1!=0:
                        Je veux te voler une plante {Reward}, merci. #speaker:Inconnu #portrait:ManOutside #layout:right
                    - else :
                        Je voulais te voler une plante {Reward} mais t'as rien ! #speaker:Inconnu #portrait:ManOutside #layout:right
                    }
                - "Move" :
                    {NbPlant2!=0:
                        Je veux te voler une plante {Reward}, merci. #speaker:Inconnu #portrait:ManOutside #layout:right
                    - else :
                        Je veux te voler de l'argent, merci. #speaker:Inconnu #portrait:ManOutside #layout:right
                    }
                - "Boost" :
                    {NbPlant3!=0:
                        Je veux te voler une plante {Reward}, merci. #speaker:Inconnu #portrait:ManOutside #layout:right
                    - else :
                        Je veux te voler de l'argent, merci. #speaker:Inconnu #portrait:ManOutside #layout:right
                    }
                }
               - else :
               Je te donne {Nbr} {Reward} ! Merci ! #speaker:Inconnu #portrait:ManOutside #layout:right
            }
    ~ StealGive(Reward, Nbr, Bonus)
    -> END
    
=== letOut ===
La personne continue de flotter dans l'espace et vous continuez votre route. #speaker:Narrateur #portrait:Narrateur #layout:left
            -> END
