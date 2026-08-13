import cliente from '../../../infraestrutura/api/cliente';

export interface Produto {
  id: string;
  codigo: string;
  nome: string;
  unidadeMedida: string;
  precoCusto: number;
  precoVenda: number;
  ativo: boolean;
  dataCadastro: string;
}

export interface SalvarProdutoRequest {
  codigo: string;
  nome: string;
  unidadeMedida: string;
  precoCusto: number;
  precoVenda: number;
  ativo: boolean;
}

export const produtosApi = {
  listar: (busca?: string, apenasAtivos = false): Promise<Produto[]> =>
    cliente
      .get<Produto[]>('/produtos', { params: { busca: busca || undefined, apenasAtivos } })
      .then((r) => r.data),

  criar: (dados: SalvarProdutoRequest): Promise<{ id: string }> =>
    cliente.post<{ id: string }>('/produtos', dados).then((r) => r.data),

  atualizar: (id: string, dados: SalvarProdutoRequest): Promise<void> =>
    cliente.put(`/produtos/${id}`, dados).then(() => undefined),

  excluir: (id: string): Promise<void> =>
    cliente.delete(`/produtos/${id}`).then(() => undefined),
};
