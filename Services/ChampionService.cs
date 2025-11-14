using System.Collections.Generic;

namespace LoLTracker.Services
{
    public class ChampionService
    {
        public List<string> GetAllChampions()
        {
            // Short list for demo. Expand as needed.
            return new List<string>
            {
                "Aatrox","Ahri","Akali","Alistar","Amumu","Anivia","Annie",
                "Ashe","Aurelion Sol","Azir","Bard","Blitzcrank","Brand","Braum",
                "Caitlyn","Camille","Cassiopeia","Cho'Gath","Corki",
                "Darius","Diana","Dr. Mundo","Draven",
                "Ekko","Elise","Evelynn","Ezreal",
                "Fiora","Fizz","Galio","Gangplank","Garen","Gnar","Gragas","Graves",
                "Hecarim","Heimerdinger","Illaoi","Irelia","Ivern",
                "Janna","Jarvan IV","Jax","Jayce","Jhin","Jinx",
                "Kai'Sa","Kalista","Karma","Karthus","Kassadin","Katarina","Kayle","Kayn",
                "Leona","Lucian","Lux","Malphite","Malzahar","Maokai","Master Yi",
                "Miss Fortune","Morgana","Nami","Nasus","Nautilus","Neeko","Nidalee",
                "Nocturne","Nunu & Willump","Olaf","Orianna","Ornn","Pantheon","Poppy",
                "Pyke","Quinn","Rakan","Rammus","Rek'Sai","Renekton","Rengar","Riven",
                "Rumble","Ryze","Sejuani","Shaco","Shen","Shyvana","Singed","Sion",
                "Sivir","Skarner","Sona","Soraka","Swain","Sylas","Syndra",
                "Tahm Kench","Taliyah","Talon","Taric","Teemo","Thresh","Tristana","Trundle",
                "Tryndamere","Twisted Fate","Twitch","Udyr","Urgot","Varus","Vayne","Veigar","Vel'Koz",
                "Vi","Viktor","Vladimir","Warwick","Wukong","Xayah","Xerath","Xin Zhao","Yasuo","Yorick","Zac","Zed","Zoe","Zyra"
            };
        }
    }
}
