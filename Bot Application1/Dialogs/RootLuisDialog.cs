using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot_Application1.Dialogs
{
    [Serializable]
    [LuisModel("d32e2a29-706d-4665-8bc6-e79cac27a6c3", "1aa58c804a56499c8f706e88158d7704")]
    public class RootLuisDialog : LuisDialog<object>
    {

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Sorry, I did not understand '{result.Query}'.");
            context.Wait(this.MessageReceived);
        }

        private List<string> startersActions = new List<string>() { "Pick Bulbasaur", "Pick Charmander", "Pick Squirtle" };
        private List<string> starters = new List<string>() { "Bulbasaur", "Charmander", "Squirtle" };

        [LuisIntent("ShowStarters")]
        public async Task Starters(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;

            message.ReplyToId = message.From.Id;
            message.Text = string.Empty;
            message.AddHeroCard("Here, take one of these rare pokemon:", startersActions, starters );
            
            await context.PostAsync(message);
        }

        [LuisIntent("PickPokemon")]
        public async Task PickPokemon(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;

            int index = startersActions.IndexOf(message.Text);
            if (index == -1)
            {
                //error
                await context.PostAsync($"You can't choose that pokemon.");
            }
            else
            {
                await context.PostAsync($"Great! You chose: **{starters[index]}**");
            }
        }


        [LuisIntent("info")]
        public async Task GetPokemonInfo(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://pokeapi.co/api/v2/");

            foreach (var entity in result.Entities)
            {
                await context.PostAsync($"Getting info for {entity.Entity}.");

                var jsonResponse = await client.GetStringAsync($"pokemon/{entity.Entity}");

                var pokemonObj = JObject.Parse(jsonResponse);

                var message = await activity;

                message.ReplyToId = message.From.Id;
                message.Text = string.Empty;

                List<CardImage> cardImages = new List<CardImage>();
                cardImages.Add(new CardImage(url: pokemonObj["sprites"]["front_default"].Value<string>()));
                cardImages.Add(new CardImage(url: pokemonObj["sprites"]["back_default"].Value<string>()));

                HeroCard infoCard = new HeroCard()
                {
                    Title = $"{pokemonObj["id"].Value<int>()} - {pokemonObj["name"].Value<string>()}",
                    Images = cardImages,
                    Subtitle = pokemonObj["name"].Value<string>(),
                    //Text = getNextEvolution(evolutionChain)
                };

                Attachment infoAttach = infoCard.ToAttachment();

                message.Attachments = new List<Attachment>();
                message.Attachments.Add(infoAttach);
                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                await context.PostAsync(message);
            }

        }

        private string getNextEvolution(dynamic evolutionChain)
        {
            //var evoJson = await client.GetStringAsync($"evolution-chain/{pokemonObj["id"].Value<int>()}");
            //var evolutionChain = JObject.Parse(evoJson)["chain"];

            return string.Empty;
        }
    }
}