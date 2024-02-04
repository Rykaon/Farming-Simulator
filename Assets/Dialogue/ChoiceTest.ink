INCLUDE globals.ink
EXTERNAL PlantBuy(PlantToBuy)
EXTERNAL PlantSell(PlantToBuy)

{ PlayerArgent>15  : ->main | ->pauvre}

=== main ===
Bienvenue au magasin ! Qu'est-ce que je peux faire pour vous ? 
-> magasin

=== magasin ===
    + [Je veux acheter une plante !]
        -> buyPlant
    + [Je veux vendre une plante !]
        -> sellPlant
    * [Au revoir !]
        Salut mon pote !
        -> END

=== buyPlant ===
Laquelle voulez-vous acheter ? ({PlayerArgent}$)
            + [Une Rouge ! (15$)]
                {PlayerArgent > 15 :
                    ~ PlayerArgent = PlayerArgent-15
                    ~ NbPlanteRouge++
                    ~ PlantBuy("Rouge")
                    Merci pour l'achat !
                    - else :
                    T'as pas assez d'argent mon pote !
                }
                ->magasin
            + [Une Bleu ! (15$)]
                {PlayerArgent > 15 :
                    ~ PlayerArgent = PlayerArgent-15
                    ~ NbPlanteBleu++
                    ~ PlantBuy("Bleu")
                    Merci pour l'achat !
                    - else :
                    T'as pas assez d'argent mon pote !
                }
                ->magasin
            + [Une Jaune ! (15$)]
                {PlayerArgent > 15 :
                    ~ PlayerArgent = PlayerArgent-15
                    ~ NbPlanteJaune++
                    ~ PlantBuy("Jaune")
                    Merci pour l'achat !
                    - else :
                    T'as pas assez d'argent mon pote !
                }
                ->magasin

=== sellPlant ===
Laquelle voulez-vous vendre ?
            + [Une Rouge ! ({NbPlanteRouge})]
                {NbPlanteRouge > 0 :
                    ~ PlayerArgent = PlayerArgent+10
                    ~ NbPlanteRouge--
                    ~ PlantSell("Rouge")
                    C'est un plaisir de faire affaire avec vous ! (+10$)
                    - else :
                    Mais ... t'as aucune Rouge !
                }
                ->magasin
            + [Une Bleu ! ({NbPlanteBleu})]
                {NbPlanteBleu > 0 :
                    ~ PlayerArgent = PlayerArgent+10
                    ~ NbPlanteBleu--
                    ~ PlantSell("Bleu")
                    C'est un plaisir de faire affaire avec vous ! (+10$)
                    - else :
                    Mais ... t'as aucune Bleu !
                }
                ->magasin
            + [Une Jaune ! ({NbPlanteJaune})]
                {NbPlanteJaune > 0 :
                    ~ PlayerArgent = PlayerArgent
                    ~ NbPlanteJaune--
                    ~ PlantSell("Jaune")
                    C'est un plaisir de faire affaire avec vous ! (+10$)
                    - else :
                    Mais ... t'as aucune Jaune !
                }
                ->magasin

=== pauvre ===
Je sais que t'es fauché, casse toi !
->magasin