INCLUDE globals.ink
EXTERNAL StealGive(RewardType, NbrReward, IsBonus)

=== main ===
Vous entendez un bruit dehors. Une personne veut rentrer. #speaker:Narrateur #portrait:? #layout:left
-> event

=== event ===
    + [Le laisser rentrer]
        -> letIn (RewardType, NbrReward, IsBonus)
    + [Le laisser dehors]
        -> letOut

=== letIn (Reward, Nbr, Bonus) ===
    Vous le laisser rentrer. #speaker:Narrateur #portrait:? #layout:left
    Hello ! #speaker:Random #portrait:?? #layout:right
            + [Bonjour !]
                ->DONE
            + [Coucou !)]
                ->DONE
               
    La personne s'approche de vous.  #speaker:Narrateur #portrait:? #layout:left
            {Bonus == false :
                {Reward: 
                - "Gold":
                    {Nbr<PlayerArgent:
                        Je veux te voler de l'argent, merci.
                    - else :
                        Je voulais te voler de l'argent, t'en a pas assez.
                    }
                - "Attack":
                    {NbPlant1!=0:
                        Je veux te voler une plante {Reward}, merci.
                    - else :
                        Je voulais te voler une plante {Reward} mais t'as rien !
                    }
                - "Move" :
                    {NbPlant2!=0:
                        Je veux te voler une plante {Reward}, merci.
                    - else :
                        Je veux te voler de l'argent, merci.
                    }
                - "Boost" :
                    {NbPlant3!=0:
                        Je veux te voler une plante {Reward}, merci.
                    - else :
                        Je veux te voler de l'argent, merci.
                    }
                }
               - else :
               Je te donne {Nbr} {Reward} ! Merci !
            }
    ~ StealGive(Reward, Nbr, Bonus)
    -> END
                

=== letOut ===
La personne continue de flotter dans l'espace et vous continuez votre route. #speaker:Narrateur #portrait:? #layout:left
            -> END
