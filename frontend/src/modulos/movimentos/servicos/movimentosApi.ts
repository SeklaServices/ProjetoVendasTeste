import cliente from '../../../infraestrutura/api/cliente';

/** Item como a tela monta antes de enviar. Subtotal e total são calculados pelo backend. */
export interface ItemRequest {
  produtoId: string;
  quantidade: number;
  precoUnitario: number;
}

export interface ItemResposta extends ItemRequest {
  id: string;
  produtoCodigo: string;
  produtoNome: string;
  unidadeMedida: string;
  subtotal: number;
}

export interface CompraResumo {
  id: string;
  numero: number;
  data: string;
  fornecedor: string;
  valorTotal: number;
  quantidadeItens: number;
}

export interface VendaResumo {
  id: string;
  numero: number;
  data: string;
  clienteId: string;
  /** Vêm resolvidos do cadastro a cada consulta — a venda guarda só o id. */
  clienteCodigo: number;
  clienteNome: string;
  valorTotal: number;
  quantidadeItens: number;
}

export interface CompraDetalhe extends CompraResumo {
  observacao: string | null;
  dataCriacao: string;
  itens: ItemResposta[];
}

export interface VendaDetalhe extends VendaResumo {
  observacao: string | null;
  dataCriacao: string;
  itens: ItemResposta[];
}

export interface Resumo {
  totalComprado: number;
  totalVendido: number;
  diferenca: number;
  quantidadeCompras: number;
  quantidadeVendas: number;
  produtosCadastrados: number;
  produtosAtivos: number;
  ultimasCompras: CompraResumo[];
  ultimasVendas: VendaResumo[];
}

export interface PeriodoFiltro {
  dataInicial?: string;
  dataFinal?: string;
}

export const comprasApi = {
  listar: (periodo: PeriodoFiltro = {}): Promise<CompraResumo[]> =>
    cliente.get<CompraResumo[]>('/compras', { params: periodo }).then((r) => r.data),

  obter: (id: string): Promise<CompraDetalhe> =>
    cliente.get<CompraDetalhe>(`/compras/${id}`).then((r) => r.data),

  criar: (dados: {
    data: string;
    fornecedor: string;
    observacao?: string | null;
    itens: ItemRequest[];
  }): Promise<{ id: string; numero: number }> =>
    cliente.post<{ id: string; numero: number }>('/compras', dados).then((r) => r.data),

  excluir: (id: string): Promise<void> =>
    cliente.delete(`/compras/${id}`).then(() => undefined),
};

export const vendasApi = {
  listar: (periodo: PeriodoFiltro = {}): Promise<VendaResumo[]> =>
    cliente.get<VendaResumo[]>('/vendas', { params: periodo }).then((r) => r.data),

  obter: (id: string): Promise<VendaDetalhe> =>
    cliente.get<VendaDetalhe>(`/vendas/${id}`).then((r) => r.data),

  criar: (dados: {
    data: string;
    clienteId: string;
    observacao?: string | null;
    itens: ItemRequest[];
  }): Promise<{ id: string; numero: number }> =>
    cliente.post<{ id: string; numero: number }>('/vendas', dados).then((r) => r.data),

  excluir: (id: string): Promise<void> =>
    cliente.delete(`/vendas/${id}`).then(() => undefined),
};

export const resumoApi = {
  obter: (periodo: PeriodoFiltro = {}): Promise<Resumo> =>
    cliente.get<Resumo>('/resumo', { params: periodo }).then((r) => r.data),
};
