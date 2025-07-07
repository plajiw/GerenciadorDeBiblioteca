using GerenciadorDeBiblioteca.Dominio.Enums;

namespace GerenciadorDeBiblioteca.Dominio.Entidades
{
    public class Livro
    {
        public string Id { get; private set; } = $"Livro-{Guid.NewGuid().ToString()}";

        public string Titulo { get; set; }

        public string Autor { get; set; }

        public GeneroLivro Genero { get; set; }

        private int _quantidadeEmEstoque;
        public int QuantidadeEmEstoque
        {
            get => _quantidadeEmEstoque;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "A quantidade em estoque não pode ser negativa.");
                _quantidadeEmEstoque = value;
            }
        }
    }
}
