import cliente from '../../../infraestrutura/api/cliente';

export interface Cliente {
  id: string;
  /** Sequencial, gerado pelo banco. Não é digitado nem editável. */
  codigo: number;
  nome: string;
  /** CPF ou CNPJ, só dígitos — a formatação é da tela. */
  documento: string | null;
  telefone: string | null;
  email: string | null;
  ativo: boolean;
  dataCadastro: string;
}

export interface SalvarClienteRequest {
  nome: string;
  documento?: string | null;
  telefone?: string | null;
  email?: string | null;
  ativo: boolean;
}

export const clientesApi = {
  listar: (busca?: string, apenasAtivos = false): Promise<Cliente[]> =>
    cliente
      .get<Cliente[]>('/clientes', { params: { busca: busca || undefined, apenasAtivos } })
      .then((r) => r.data),

  criar: (dados: SalvarClienteRequest): Promise<{ id: string }> =>
    cliente.post<{ id: string }>('/clientes', dados).then((r) => r.data),

  atualizar: (id: string, dados: SalvarClienteRequest): Promise<void> =>
    cliente.put(`/clientes/${id}`, dados).then(() => undefined),

  excluir: (id: string): Promise<void> =>
    cliente.delete(`/clientes/${id}`).then(() => undefined),
};

/** Aplica a máscara de CPF ou CNPJ. Fora disso, devolve como veio. */
export function formatarDocumento(documento: string | null): string {
  if (!documento) {
    return '';
  }

  if (documento.length === 11) {
    return documento.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
  }

  if (documento.length === 14) {
    return documento.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
  }

  return documento;
}
