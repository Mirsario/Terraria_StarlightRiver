﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Alchemy
{
    public abstract class CauldronDummyAbstract : Dummy
    {
        //This serves as the core logic driver of the alchemy system
        //any inputs and outputs will be routed through here
        //and this will execute the calls to any ingredient logic and visuals
        //Abstract so this can be overridden by more specific cauldrons if later there are multiple cauldrons with similar logic but different visuals

        protected List<AlchemyIngredient> currentIngredients = new List<AlchemyIngredient>();
        protected List<int> currentModifiers = new List<int>();

        List<AlchemyRecipe> possibleRecipes = AlchemyRecipeSystem.recipeList;

        AlchemyRecipe currentRecipe = null;
        

        AlchemyIngredient mostRecentIngredient = null;

        AlchemyWrapper wrapper = new AlchemyWrapper();

        protected bool isCrafting = false; //true when the ingredients are finalized and recipe is doing visuals/crafting

        public const int bubbleAnimationFrameTime = 8;
        public const int bubbleAnimationFrames = 10;
        public const int bubbleYOffset = 10; //bubble animation is centered in their frames so we need an offset to find bottom

        public int inputCooldown = 0; //if this is greater than 0, the cauldron will not take inputs until it reaches 0 again

        protected CauldronDummyAbstract(int validType, int width, int height) : base(validType, width, height)
        {
        }

        public override void Update()
        {
            if (!isCrafting && inputCooldown <= 0)
            {
                foreach (Item eachWorldItem in Main.item)
                {
                    if (eachWorldItem.active && projectile.Hitbox.Contains(eachWorldItem.Center.ToPoint()))
                    {
                        if (AttemptAddItem(eachWorldItem.Clone()))
                        {
                            //todo: mp logic
                            eachWorldItem.active = false;
                            eachWorldItem.TurnToAir();
                        }
                    }
                }
            }

            wrapper.bubbleColor = new Color(127, 127, 127);
            wrapper.cauldronRect = projectile.Hitbox;

            bool skipIngredientLogic = false;
            if (currentRecipe != null)
            {
                if(isCrafting)
                {
                    skipIngredientLogic = currentRecipe.UpdateCrafting(wrapper, currentIngredients, this);
                } else if (wrapper.currentBatchSize > 0)
                {
                    skipIngredientLogic = currentRecipe.UpdateReady(wrapper);
                } else
                {
                    skipIngredientLogic = currentRecipe.updateAlmostReady(wrapper);
                }

            }


            if (mostRecentIngredient != null && !skipIngredientLogic)
            {
                bool ignoreRegularVisuals = mostRecentIngredient.mostRecentUpdate(wrapper);

                foreach (AlchemyIngredient ingredient in currentIngredients)
                {
                    ingredient.Update(wrapper);
                    if (!ignoreRegularVisuals)
                        ingredient.visualUpdate(wrapper);
                    ingredient.incrementTimer();
                }

                mostRecentIngredient.mostRecentPostUpdate(wrapper);
            }


            incrementWrapperTimers();

            if (inputCooldown > 0)
                inputCooldown--;
        }

        protected virtual void incrementWrapperTimers()
        {
            wrapper.bubbleAnimationTimer++;
            wrapper.timeSinceCraftStarted++;
            wrapper.timeSinceCraftReady++;

            if (wrapper.bubbleAnimationTimer >= bubbleAnimationFrameTime)
            {
                wrapper.bubbleAnimationTimer = 0;
                wrapper.bubbleAnimationFrame++;
                wrapper.bubbleAnimationFrame %= bubbleAnimationFrames;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (mostRecentIngredient != null && wrapper.bubbleOpacity > 0f)
            {
                Texture2D bubbleSheet = GetTexture(AssetDirectory.Alchemy + "BubbleSheet");
                Texture2D bubbleGlow = GetTexture(AssetDirectory.Alchemy + "BubbleSheetGlow");
                int frameHeight = bubbleSheet.Height / bubbleAnimationFrames;

                if (wrapper.bubbleOpacity > 1f)
                    wrapper.bubbleOpacity = 1f;

                wrapper.bubbleColor.A = (byte)(wrapper.bubbleColor.A * wrapper.bubbleOpacity);
                spriteBatch.Draw(bubbleSheet, projectile.position - Main.screenPosition - new Vector2(0, frameHeight - bubbleYOffset), new Rectangle(0, frameHeight * wrapper.bubbleAnimationFrame, bubbleSheet.Width, frameHeight), wrapper.bubbleColor);

                spriteBatch.End();
                spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);

                spriteBatch.Draw(bubbleGlow, projectile.position - Main.screenPosition - new Vector2(0, frameHeight - bubbleYOffset), new Rectangle(0, frameHeight * wrapper.bubbleAnimationFrame, bubbleSheet.Width, frameHeight), wrapper.bubbleColor * wrapper.bubbleOpacity);

                spriteBatch.End();
                spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            }

            return false;
        }

        /// <summary>
        /// empties out cauldron and dumps items into the world and resets any data like possible recipes
        /// </summary>
        public void dumpIngredients()
        {
            possibleRecipes = AlchemyRecipeSystem.recipeList;
            mostRecentIngredient = null;
            currentRecipe = null;
            isCrafting = false;
            wrapper.currentBatchSize = 0;

            foreach (AlchemyIngredient ingredient in currentIngredients)
            {
                ingredient.dump(wrapper.cauldronRect);
            }

            currentIngredients.Clear();
            inputCooldown = 120;
        }


        public void consumeAndDumpIngredients()
        {

        }

        /// <summary>
        /// attempts to insert a specific item into the cauldron. if cannot be added returns false and performs no additional logic.
        /// if can be added will create and add ingredient to the current ingredients, and update possibleRecipes, returning true
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool AttemptAddItem(Item item)
        {
            List<AlchemyRecipe> newPossibilities =  AlchemyRecipeSystem.getRemainingPossiblities(item, possibleRecipes);
            if (newPossibilities != null && newPossibilities.Count >= 1)
            {
                //first attempts to stack into an existing ingredient stack
                bool hasStack = false;
                for (int i = 0; i < currentIngredients.Count; i++)
                {
                    AlchemyIngredient eachIngredient = currentIngredients[i];
                    if (eachIngredient.getItemId() == item.type)
                    {
                        if (eachIngredient.addToStack(item))
                        {
                            mostRecentIngredient = eachIngredient;

                            //move to end of list
                            currentIngredients.RemoveAt(i);
                            currentIngredients.Add(eachIngredient);

                            hasStack = true;
                            break;
                        } else
                        {
                            //if an ingredient stack is found but not stackable we can skip and return false
                            return false;
                        }
                    }
                }

                if (!hasStack)
                {
                    AlchemyIngredient newIngredient = AlchemyRecipeSystem.instantiateIngredient(item);

                    mostRecentIngredient = newIngredient;

                    currentIngredients.Add(newIngredient);
                }

                possibleRecipes = newPossibilities;

                //TODO: mp logic here ?
                //if it reaches here, means that items were successfully added to the cauldron, so we check for full validation on the recipes to see if theres only 1 and its ready
                if (possibleRecipes.Count == 1)
                {
                    currentRecipe = possibleRecipes[0];
                    wrapper.currentBatchSize = currentRecipe.getCraftBatchSize(currentIngredients, currentModifiers);
                } else
                {
                    currentRecipe = null;
                    wrapper.currentBatchSize = 0;
                }

                return true;
            }
            
            return false;
        }

        public bool AttemptStartCraft()
        {
            if (currentRecipe != null && wrapper.currentBatchSize > 0 && !isCrafting)
            {
                wrapper.timeSinceCraftStarted = 0;
                isCrafting = true;
                return true;
            }
            return false;
        }
    }
}